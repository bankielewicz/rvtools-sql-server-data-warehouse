namespace RVToolsService.Services;

/// <summary>
/// Service for managing Quartz.NET scheduled jobs.
/// Loads job configurations from Service.Jobs table and registers them with Quartz.NET.
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Starts the Quartz.NET scheduler and loads all enabled scheduled jobs.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the Quartz.NET scheduler.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads all jobs from the database.
    /// Call this when job configurations change.
    /// </summary>
    Task ReloadJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules or reschedules a specific job.
    /// </summary>
    Task ScheduleJobAsync(int jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unschedules a specific job.
    /// </summary>
    Task UnscheduleJobAsync(int jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next fire time for a job.
    /// </summary>
    Task<DateTime?> GetNextFireTimeAsync(int jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the scheduler is running.
    /// </summary>
    bool IsRunning { get; }
}
