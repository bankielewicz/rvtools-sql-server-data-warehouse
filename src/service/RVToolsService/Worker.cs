using RVToolsService.Services;

namespace RVToolsService;

/// <summary>
/// Main background worker for the RVTools Import Service.
/// Runs continuously as a Windows Service, handling:
/// - Manual trigger polling (Phase 2)
/// - Scheduled imports via Quartz.NET (Phase 3)
/// - File watching for incoming files (Phase 4)
/// - Service health heartbeats
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISchedulerService _schedulerService;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ISchedulerService schedulerService)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _schedulerService = schedulerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RVTools Import Service starting at: {time}", DateTimeOffset.Now);

        // Start Quartz.NET scheduler and load jobs from database
        try
        {
            await _schedulerService.StartAsync(stoppingToken);
            _logger.LogInformation("Quartz.NET scheduler started - scheduled jobs are now active");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Quartz.NET scheduler: {Error}", ex.Message);
            // Continue running - manual triggers will still work
        }

        // Read intervals from configuration
        var heartbeatInterval = _configuration.GetValue("ServiceSettings:HeartbeatIntervalSeconds", 30);
        var triggerPollInterval = _configuration.GetValue("ServiceSettings:ManualTriggerPollSeconds", 10);

        var lastHeartbeat = DateTime.MinValue;
        var heartbeatSpan = TimeSpan.FromSeconds(heartbeatInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Update heartbeat at the configured interval
                if (DateTime.UtcNow - lastHeartbeat >= heartbeatSpan)
                {
                    await UpdateHeartbeatAsync(stoppingToken);
                    lastHeartbeat = DateTime.UtcNow;
                }

                // Check for manual triggers
                await CheckManualTriggersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in service main loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(triggerPollInterval), stoppingToken);
        }

        // Stop Quartz.NET scheduler
        try
        {
            await _schedulerService.StopAsync(stoppingToken);
            _logger.LogInformation("Quartz.NET scheduler stopped");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping Quartz.NET scheduler: {Error}", ex.Message);
        }

        _logger.LogInformation("RVTools Import Service stopping at: {time}", DateTimeOffset.Now);
    }

    /// <summary>
    /// Checks for pending manual triggers and processes them.
    /// </summary>
    private async Task CheckManualTriggersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var triggerService = scope.ServiceProvider.GetRequiredService<IJobTriggerService>();
        var importService = scope.ServiceProvider.GetRequiredService<IImportJobService>();

        try
        {
            var pendingTriggers = await triggerService.GetPendingTriggersAsync();

            if (pendingTriggers.Count == 0)
                return;

            _logger.LogInformation("Found {Count} pending trigger(s)", pendingTriggers.Count);

            foreach (var trigger in pendingTriggers)
            {
                stoppingToken.ThrowIfCancellationRequested();

                _logger.LogInformation(
                    "Processing trigger {TriggerId} for job '{JobName}' (type: {TriggerType}, user: {User})",
                    trigger.TriggerId, trigger.JobName, trigger.TriggerType, trigger.TriggerUser ?? "system");

                try
                {
                    // Get full job configuration
                    var job = await triggerService.GetJobAsync(trigger.JobId);

                    if (job == null)
                    {
                        _logger.LogWarning("Job {JobId} not found for trigger {TriggerId}", trigger.JobId, trigger.TriggerId);
                        await triggerService.MarkTriggerProcessedAsync(trigger.TriggerId);
                        continue;
                    }

                    if (!job.IsEnabled)
                    {
                        _logger.LogWarning("Job '{JobName}' is disabled, skipping trigger {TriggerId}",
                            job.JobName, trigger.TriggerId);
                        await triggerService.MarkTriggerProcessedAsync(trigger.TriggerId);
                        continue;
                    }

                    // Execute the job
                    var result = await importService.ExecuteJobAsync(
                        job,
                        trigger.TriggerType,
                        trigger.TriggerUser,
                        stoppingToken);

                    _logger.LogInformation(
                        "Job '{JobName}' completed: Status={Status}, Files={Processed}/{Total}",
                        job.JobName, result.Status, result.FilesProcessed, result.FilesProcessed + result.FilesFailed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process trigger {TriggerId}: {Error}",
                        trigger.TriggerId, ex.Message);
                }
                finally
                {
                    // Always mark trigger as processed
                    await triggerService.MarkTriggerProcessedAsync(trigger.TriggerId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking for manual triggers: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// Updates the service health heartbeat in the database.
    /// </summary>
    private async Task UpdateHeartbeatAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var healthService = scope.ServiceProvider.GetRequiredService<IServiceHealthService>();

        try
        {
            await healthService.UpdateHeartbeatAsync(stoppingToken);
            _logger.LogDebug("Service heartbeat updated at: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update heartbeat: {Error}", ex.Message);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RVTools Import Service is starting...");

        // Update initial heartbeat status
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var healthService = scope.ServiceProvider.GetRequiredService<IServiceHealthService>();
            await healthService.UpdateStatusAsync("Running", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update initial service status: {Error}", ex.Message);
        }

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RVTools Import Service is stopping...");

        // Update status to Stopped
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var healthService = scope.ServiceProvider.GetRequiredService<IServiceHealthService>();
            await healthService.UpdateStatusAsync("Stopped", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update service status on stop: {Error}", ex.Message);
        }

        await base.StopAsync(cancellationToken);
    }
}
