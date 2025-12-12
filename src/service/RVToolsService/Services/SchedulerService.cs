using Dapper;
using Quartz;
using Quartz.Impl.Matchers;
using RVToolsService.Jobs;
using RVToolsShared.Data;
using RVToolsShared.Models;

namespace RVToolsService.Services;

/// <summary>
/// Manages Quartz.NET scheduled jobs, loading configurations from Service.Jobs table.
/// </summary>
public class SchedulerService : ISchedulerService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<SchedulerService> _logger;
    private IScheduler? _scheduler;

    private const string JobGroup = "RVToolsImports";
    private const string TriggerGroup = "RVToolsTriggers";

    public SchedulerService(
        ISchedulerFactory schedulerFactory,
        ISqlConnectionFactory connectionFactory,
        ILogger<SchedulerService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public bool IsRunning => _scheduler?.IsStarted == true && !_scheduler.IsShutdown;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Quartz.NET scheduler...");

        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        // Load and schedule all enabled jobs
        await LoadAndScheduleJobsAsync(cancellationToken);

        await _scheduler.Start(cancellationToken);

        _logger.LogInformation("Quartz.NET scheduler started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) return;

        _logger.LogInformation("Stopping Quartz.NET scheduler...");

        // Wait for running jobs to complete (with timeout)
        await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);

        _logger.LogInformation("Quartz.NET scheduler stopped");
    }

    public async Task ReloadJobsAsync(CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
        {
            _logger.LogWarning("Cannot reload jobs - scheduler not started");
            return;
        }

        _logger.LogInformation("Reloading all scheduled jobs...");

        // Clear existing jobs
        var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(JobGroup), cancellationToken);
        foreach (var jobKey in jobKeys)
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken);
        }

        // Reload from database
        await LoadAndScheduleJobsAsync(cancellationToken);

        _logger.LogInformation("Scheduled jobs reloaded");
    }

    public async Task ScheduleJobAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null)
        {
            _logger.LogWarning("Cannot schedule job - scheduler not started");
            return;
        }

        // First remove existing schedule if any
        await UnscheduleJobAsync(jobId, cancellationToken);

        // Get job from database
        var job = await GetJobFromDatabaseAsync(jobId);

        if (job == null)
        {
            _logger.LogWarning("Job {JobId} not found in database", jobId);
            return;
        }

        if (!job.IsEnabled)
        {
            _logger.LogInformation("Job '{JobName}' ({JobId}) is disabled, not scheduling", job.JobName, jobId);
            return;
        }

        if (string.IsNullOrWhiteSpace(job.CronSchedule))
        {
            _logger.LogInformation("Job '{JobName}' ({JobId}) has no cron schedule, not scheduling", job.JobName, jobId);
            return;
        }

        await ScheduleJobInternalAsync(job, cancellationToken);
    }

    public async Task UnscheduleJobAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) return;

        var jobKey = new JobKey($"Job_{jobId}", JobGroup);
        var exists = await _scheduler.CheckExists(jobKey, cancellationToken);

        if (exists)
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken);
            _logger.LogInformation("Unscheduled job {JobId}", jobId);
        }
    }

    public async Task<DateTime?> GetNextFireTimeAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (_scheduler == null) return null;

        var triggerKey = new TriggerKey($"Trigger_{jobId}", TriggerGroup);
        var trigger = await _scheduler.GetTrigger(triggerKey, cancellationToken);

        return trigger?.GetNextFireTimeUtc()?.LocalDateTime;
    }

    private async Task LoadAndScheduleJobsAsync(CancellationToken cancellationToken)
    {
        var jobs = await GetEnabledScheduledJobsAsync();
        var scheduledCount = 0;
        var skippedCount = 0;

        foreach (var job in jobs)
        {
            if (string.IsNullOrWhiteSpace(job.CronSchedule))
            {
                _logger.LogDebug("Job '{JobName}' has no cron schedule, skipping", job.JobName);
                skippedCount++;
                continue;
            }

            try
            {
                await ScheduleJobInternalAsync(job, cancellationToken);
                scheduledCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule job '{JobName}' ({JobId}): {Error}",
                    job.JobName, job.JobId, ex.Message);
                skippedCount++;
            }
        }

        _logger.LogInformation("Loaded {Scheduled} scheduled jobs ({Skipped} skipped)", scheduledCount, skippedCount);
    }

    private async Task ScheduleJobInternalAsync(JobDto job, CancellationToken cancellationToken)
    {
        if (_scheduler == null) return;

        var jobKey = new JobKey($"Job_{job.JobId}", JobGroup);
        var triggerKey = new TriggerKey($"Trigger_{job.JobId}", TriggerGroup);

        // Create Quartz job detail
        var jobDetail = JobBuilder.Create<ImportJob>()
            .WithIdentity(jobKey)
            .WithDescription($"Import job: {job.JobName}")
            .UsingJobData(ImportJob.JobIdKey, job.JobId.ToString())
            .StoreDurably()
            .Build();

        // Parse timezone
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(job.TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogWarning("TimeZone '{TimeZone}' not found for job '{JobName}', using UTC",
                job.TimeZone, job.JobName);
            timeZone = TimeZoneInfo.Utc;
        }

        // Create cron trigger
        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .WithDescription($"Trigger for: {job.JobName}")
            .ForJob(jobKey)
            .WithCronSchedule(job.CronSchedule!, x => x
                .InTimeZone(timeZone)
                .WithMisfireHandlingInstructionDoNothing()) // Skip misfired executions
            .Build();

        // Schedule the job
        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);

        var nextFire = trigger.GetNextFireTimeUtc()?.LocalDateTime;
        _logger.LogInformation(
            "Scheduled job '{JobName}' ({JobId}) with cron '{Cron}' (timezone: {TZ}). Next fire: {NextFire}",
            job.JobName, job.JobId, job.CronSchedule, job.TimeZone, nextFire);
    }

    private async Task<IEnumerable<JobDto>> GetEnabledScheduledJobsAsync()
    {
        const string sql = @"
            SELECT
                JobId, JobName, JobType, IsEnabled,
                IncomingFolder, ProcessedFolder, ErrorsFolder,
                CronSchedule, TimeZone,
                ServerInstance, DatabaseName, UseWindowsAuth, EncryptedCredential,
                VIServer, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
            FROM [Service].[Jobs]
            WHERE IsEnabled = 1 AND JobType = 'Scheduled'
            ORDER BY JobName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<JobDto>(sql);
    }

    private async Task<JobDto?> GetJobFromDatabaseAsync(int jobId)
    {
        const string sql = @"
            SELECT
                JobId, JobName, JobType, IsEnabled,
                IncomingFolder, ProcessedFolder, ErrorsFolder,
                CronSchedule, TimeZone,
                ServerInstance, DatabaseName, UseWindowsAuth, EncryptedCredential,
                VIServer, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate
            FROM [Service].[Jobs]
            WHERE JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<JobDto>(sql, new { JobId = jobId });
    }
}
