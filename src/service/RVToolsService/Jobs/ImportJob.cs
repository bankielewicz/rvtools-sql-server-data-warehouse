using Quartz;
using RVToolsService.Services;

namespace RVToolsService.Jobs;

/// <summary>
/// Quartz.NET job that executes scheduled RVTools imports.
/// Each job instance is associated with a Service.Jobs record via JobId in JobDataMap.
/// </summary>
[DisallowConcurrentExecution]
public class ImportJob : IJob
{
    private readonly ILogger<ImportJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Key used to store the JobId in Quartz JobDataMap.
    /// </summary>
    public const string JobIdKey = "RVToolsJobId";

    public ImportJob(ILogger<ImportJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobIdValue = context.JobDetail.JobDataMap.GetString(JobIdKey);

        if (string.IsNullOrEmpty(jobIdValue) || !int.TryParse(jobIdValue, out var jobId))
        {
            _logger.LogError("ImportJob executed without valid JobId in JobDataMap");
            return;
        }

        _logger.LogInformation(
            "Scheduled import job {JobId} starting (FireTime: {FireTime}, ScheduledTime: {ScheduledTime})",
            jobId,
            context.FireTimeUtc.LocalDateTime,
            context.ScheduledFireTimeUtc?.LocalDateTime);

        try
        {
            // Create a scope for scoped services
            using var scope = _serviceProvider.CreateScope();
            var triggerService = scope.ServiceProvider.GetRequiredService<IJobTriggerService>();
            var importService = scope.ServiceProvider.GetRequiredService<IImportJobService>();

            // Get full job configuration from database
            var job = await triggerService.GetJobAsync(jobId);

            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found in database, skipping scheduled execution", jobId);
                return;
            }

            if (!job.IsEnabled)
            {
                _logger.LogInformation("Job '{JobName}' ({JobId}) is disabled, skipping scheduled execution",
                    job.JobName, jobId);
                return;
            }

            // Execute the import job
            var result = await importService.ExecuteJobAsync(
                job,
                triggerType: "Scheduled",
                triggerUser: null,
                context.CancellationToken);

            _logger.LogInformation(
                "Scheduled job '{JobName}' completed: Status={Status}, Files={Processed}/{Total}, Duration={Duration}ms",
                job.JobName,
                result.Status,
                result.FilesProcessed,
                result.FilesProcessed + result.FilesFailed,
                result.DurationMs);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Scheduled job {JobId} was cancelled", jobId);
            throw; // Re-throw to let Quartz handle cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled job {JobId} failed with error: {Error}", jobId, ex.Message);
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}
