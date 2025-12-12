using Dapper;
using RVToolsShared.Data;
using RVToolsShared.Models;

namespace RVToolsService.Services;

/// <summary>
/// Service for managing job triggers and job queries.
/// </summary>
public class JobTriggerService : IJobTriggerService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<JobTriggerService> _logger;

    public JobTriggerService(ISqlConnectionFactory connectionFactory, ILogger<JobTriggerService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<PendingTrigger>> GetPendingTriggersAsync()
    {
        const string sql = @"
            SELECT
                t.TriggerId,
                t.JobId,
                t.TriggerType,
                t.TriggerUser,
                t.CreatedDate,
                j.JobName,
                j.IsEnabled
            FROM [Service].[JobTriggers] t
            INNER JOIN [Service].[Jobs] j ON t.JobId = j.JobId
            WHERE t.ProcessedDate IS NULL
            AND j.IsEnabled = 1
            ORDER BY t.CreatedDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var triggers = await connection.QueryAsync<PendingTrigger>(sql);
        return triggers.ToList();
    }

    /// <inheritdoc/>
    public async Task MarkTriggerProcessedAsync(long triggerId)
    {
        const string sql = @"
            UPDATE [Service].[JobTriggers]
            SET ProcessedDate = GETUTCDATE()
            WHERE TriggerId = @TriggerId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { TriggerId = triggerId });
        _logger.LogDebug("Marked trigger {TriggerId} as processed", triggerId);
    }

    /// <inheritdoc/>
    public async Task<JobDto?> GetJobAsync(int jobId)
    {
        const string sql = @"
            SELECT
                JobId,
                JobName,
                JobType,
                IsEnabled,
                IncomingFolder,
                ProcessedFolder,
                ErrorsFolder,
                CronSchedule,
                TimeZone,
                ServerInstance,
                DatabaseName,
                UseWindowsAuth,
                EncryptedCredential,
                VIServer,
                CreatedBy,
                CreatedDate,
                ModifiedBy,
                ModifiedDate
            FROM [Service].[Jobs]
            WHERE JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        return await connection.QuerySingleOrDefaultAsync<JobDto>(sql, new { JobId = jobId });
    }

    /// <inheritdoc/>
    public async Task<List<JobDto>> GetEnabledJobsAsync()
    {
        const string sql = @"
            SELECT
                JobId,
                JobName,
                JobType,
                IsEnabled,
                IncomingFolder,
                ProcessedFolder,
                ErrorsFolder,
                CronSchedule,
                TimeZone,
                ServerInstance,
                DatabaseName,
                UseWindowsAuth,
                EncryptedCredential,
                VIServer,
                CreatedBy,
                CreatedDate,
                ModifiedBy,
                ModifiedDate
            FROM [Service].[Jobs]
            WHERE IsEnabled = 1
            ORDER BY JobName";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var jobs = await connection.QueryAsync<JobDto>(sql);
        return jobs.ToList();
    }
}
