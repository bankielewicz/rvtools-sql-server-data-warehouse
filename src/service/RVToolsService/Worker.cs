namespace RVToolsService;

/// <summary>
/// Main background worker for the RVTools Import Service.
/// Runs continuously as a Windows Service, handling:
/// - Scheduled imports via Quartz.NET (Phase 3)
/// - File watching for incoming files (Phase 4)
/// - Manual trigger polling (Phase 2)
/// - Service health heartbeats
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RVTools Import Service starting at: {time}", DateTimeOffset.Now);

        // Read heartbeat interval from configuration (default 30 seconds)
        var heartbeatInterval = _configuration.GetValue<int>("ServiceSettings:HeartbeatIntervalSeconds", 30);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Phase 1: Basic heartbeat logging
                _logger.LogInformation("Service heartbeat at: {time}", DateTimeOffset.Now);

                // Phase 2: Check for manual triggers (to be implemented)
                // await CheckManualTriggersAsync(stoppingToken);

                // Phase 3: Quartz.NET scheduler will handle cron jobs

                // Phase 4: File watcher will handle incoming files
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in service heartbeat loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(heartbeatInterval), stoppingToken);
        }

        _logger.LogInformation("RVTools Import Service stopping at: {time}", DateTimeOffset.Now);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RVTools Import Service is starting...");
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RVTools Import Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
