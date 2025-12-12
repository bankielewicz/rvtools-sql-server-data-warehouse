using System.Text.Json;
using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;
using RVToolsWeb.Services.Auth;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service for managing import jobs (Service.Jobs, JobRuns, JobTriggers tables).
/// </summary>
public class JobManagementService : IJobManagementService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICredentialProtectionService _credentialService;
    private readonly ILogger<JobManagementService> _logger;

    public JobManagementService(
        ISqlConnectionFactory connectionFactory,
        ICredentialProtectionService credentialService,
        ILogger<JobManagementService> logger)
    {
        _connectionFactory = connectionFactory;
        _credentialService = credentialService;
        _logger = logger;
    }

    /// <summary>
    /// Encrypts SQL credentials as JSON structure.
    /// </summary>
    private string? EncryptCredentials(string username, string password)
    {
        var json = JsonSerializer.Serialize(new { username, password });
        return _credentialService.Encrypt(json);
    }

    #region Jobs

    public async Task<IEnumerable<JobViewModel>> GetAllJobsAsync()
    {
        const string sql = @"
            SELECT
                j.JobId,
                j.JobName,
                j.JobType,
                j.IsEnabled,
                j.IncomingFolder,
                j.ProcessedFolder,
                j.ErrorsFolder,
                j.CronSchedule,
                j.TimeZone,
                j.ServerInstance,
                j.DatabaseName,
                j.UseWindowsAuth,
                CASE WHEN j.EncryptedCredential IS NOT NULL THEN 1 ELSE 0 END AS HasCredential,
                j.VIServer,
                j.CreatedBy,
                j.CreatedDate,
                j.ModifiedBy,
                j.ModifiedDate,
                lr.StartTime AS LastRunTime,
                lr.Status AS LastRunStatus
            FROM [Service].[Jobs] j
            OUTER APPLY (
                SELECT TOP 1 StartTime, Status
                FROM [Service].[JobRuns] r
                WHERE r.JobId = j.JobId
                ORDER BY r.StartTime DESC
            ) lr
            ORDER BY j.JobName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<JobViewModel>(sql);
    }

    public async Task<JobViewModel?> GetJobByIdAsync(int jobId)
    {
        const string sql = @"
            SELECT
                j.JobId,
                j.JobName,
                j.JobType,
                j.IsEnabled,
                j.IncomingFolder,
                j.ProcessedFolder,
                j.ErrorsFolder,
                j.CronSchedule,
                j.TimeZone,
                j.ServerInstance,
                j.DatabaseName,
                j.UseWindowsAuth,
                CASE WHEN j.EncryptedCredential IS NOT NULL THEN 1 ELSE 0 END AS HasCredential,
                j.VIServer,
                j.CreatedBy,
                j.CreatedDate,
                j.ModifiedBy,
                j.ModifiedDate,
                lr.StartTime AS LastRunTime,
                lr.Status AS LastRunStatus
            FROM [Service].[Jobs] j
            OUTER APPLY (
                SELECT TOP 1 StartTime, Status
                FROM [Service].[JobRuns] r
                WHERE r.JobId = j.JobId
                ORDER BY r.StartTime DESC
            ) lr
            WHERE j.JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<JobViewModel>(sql, new { JobId = jobId });
    }

    public async Task<int> CreateJobAsync(JobEditViewModel model, string createdBy)
    {
        // Encrypt credentials if provided
        string? encryptedCredential = null;
        if (!model.UseWindowsAuth && !string.IsNullOrEmpty(model.SqlUsername) && !string.IsNullOrEmpty(model.SqlPassword))
        {
            encryptedCredential = EncryptCredentials(model.SqlUsername, model.SqlPassword);
        }

        const string sql = @"
            INSERT INTO [Service].[Jobs] (
                JobName, JobType, IsEnabled,
                IncomingFolder, ProcessedFolder, ErrorsFolder,
                CronSchedule, TimeZone,
                ServerInstance, DatabaseName, UseWindowsAuth, EncryptedCredential,
                VIServer,
                CreatedBy, CreatedDate
            )
            OUTPUT INSERTED.JobId
            VALUES (
                @JobName, @JobType, @IsEnabled,
                @IncomingFolder, @ProcessedFolder, @ErrorsFolder,
                @CronSchedule, @TimeZone,
                @ServerInstance, @DatabaseName, @UseWindowsAuth, @EncryptedCredential,
                @VIServer,
                @CreatedBy, GETUTCDATE()
            )";

        using var connection = _connectionFactory.CreateConnection();
        var jobId = await connection.ExecuteScalarAsync<int>(sql, new
        {
            model.JobName,
            model.JobType,
            model.IsEnabled,
            model.IncomingFolder,
            model.ProcessedFolder,
            model.ErrorsFolder,
            model.CronSchedule,
            model.TimeZone,
            model.ServerInstance,
            model.DatabaseName,
            model.UseWindowsAuth,
            EncryptedCredential = encryptedCredential,
            model.VIServer,
            CreatedBy = createdBy
        });

        _logger.LogInformation("Created job {JobId} '{JobName}' by {User}", jobId, model.JobName, createdBy);
        return jobId;
    }

    public async Task<bool> UpdateJobAsync(JobEditViewModel model, string modifiedBy)
    {
        // Build dynamic SQL to conditionally update credentials
        var updateCredential = !model.UseWindowsAuth
            && !string.IsNullOrEmpty(model.SqlUsername)
            && !string.IsNullOrEmpty(model.SqlPassword);

        string? encryptedCredential = null;
        if (updateCredential)
        {
            encryptedCredential = EncryptCredentials(model.SqlUsername!, model.SqlPassword!);
        }

        var sql = @"
            UPDATE [Service].[Jobs]
            SET JobName = @JobName,
                JobType = @JobType,
                IsEnabled = @IsEnabled,
                IncomingFolder = @IncomingFolder,
                ProcessedFolder = @ProcessedFolder,
                ErrorsFolder = @ErrorsFolder,
                CronSchedule = @CronSchedule,
                TimeZone = @TimeZone,
                ServerInstance = @ServerInstance,
                DatabaseName = @DatabaseName,
                UseWindowsAuth = @UseWindowsAuth,
                VIServer = @VIServer,
                ModifiedBy = @ModifiedBy,
                ModifiedDate = GETUTCDATE()" +
            (updateCredential ? ", EncryptedCredential = @EncryptedCredential" : "") +
            (model.UseWindowsAuth ? ", EncryptedCredential = NULL" : "") +
            " WHERE JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            model.JobId,
            model.JobName,
            model.JobType,
            model.IsEnabled,
            model.IncomingFolder,
            model.ProcessedFolder,
            model.ErrorsFolder,
            model.CronSchedule,
            model.TimeZone,
            model.ServerInstance,
            model.DatabaseName,
            model.UseWindowsAuth,
            model.VIServer,
            ModifiedBy = modifiedBy,
            EncryptedCredential = encryptedCredential
        });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Updated job {JobId} '{JobName}' by {User}", model.JobId, model.JobName, modifiedBy);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        const string sql = "DELETE FROM [Service].[Jobs] WHERE JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Deleted job {JobId}", jobId);
        }

        return rowsAffected > 0;
    }

    public async Task<bool> SetJobEnabledAsync(int jobId, bool isEnabled)
    {
        const string sql = @"
            UPDATE [Service].[Jobs]
            SET IsEnabled = @IsEnabled,
                ModifiedDate = GETUTCDATE()
            WHERE JobId = @JobId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId, IsEnabled = isEnabled });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("{Action} job {JobId}", isEnabled ? "Enabled" : "Disabled", jobId);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Job Runs

    public async Task<IEnumerable<JobRunViewModel>> GetJobRunsAsync(int jobId, int limit = 50)
    {
        const string sql = @"
            SELECT TOP (@Limit)
                r.JobRunId,
                r.JobId,
                j.JobName,
                r.ImportBatchId,
                r.TriggerType,
                r.TriggerUser,
                r.StartTime,
                r.EndTime,
                r.DurationMs,
                r.Status,
                r.FilesProcessed,
                r.FilesFailed,
                r.ErrorMessage
            FROM [Service].[JobRuns] r
            INNER JOIN [Service].[Jobs] j ON r.JobId = j.JobId
            WHERE r.JobId = @JobId
            ORDER BY r.StartTime DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<JobRunViewModel>(sql, new { JobId = jobId, Limit = limit });
    }

    public async Task<IEnumerable<JobRunViewModel>> GetRecentJobRunsAsync(int limit = 50)
    {
        const string sql = @"
            SELECT TOP (@Limit)
                r.JobRunId,
                r.JobId,
                j.JobName,
                r.ImportBatchId,
                r.TriggerType,
                r.TriggerUser,
                r.StartTime,
                r.EndTime,
                r.DurationMs,
                r.Status,
                r.FilesProcessed,
                r.FilesFailed,
                r.ErrorMessage
            FROM [Service].[JobRuns] r
            INNER JOIN [Service].[Jobs] j ON r.JobId = j.JobId
            ORDER BY r.StartTime DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<JobRunViewModel>(sql, new { Limit = limit });
    }

    public async Task<JobRunViewModel?> GetJobRunByIdAsync(long jobRunId)
    {
        const string sql = @"
            SELECT
                r.JobRunId,
                r.JobId,
                j.JobName,
                r.ImportBatchId,
                r.TriggerType,
                r.TriggerUser,
                r.StartTime,
                r.EndTime,
                r.DurationMs,
                r.Status,
                r.FilesProcessed,
                r.FilesFailed,
                r.ErrorMessage
            FROM [Service].[JobRuns] r
            INNER JOIN [Service].[Jobs] j ON r.JobId = j.JobId
            WHERE r.JobRunId = @JobRunId";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<JobRunViewModel>(sql, new { JobRunId = jobRunId });
    }

    #endregion

    #region Triggers

    public async Task<bool> TriggerJobNowAsync(int jobId, string triggerUser)
    {
        const string sql = @"
            INSERT INTO [Service].[JobTriggers] (JobId, TriggerType, TriggerUser, CreatedDate)
            VALUES (@JobId, 'Manual', @TriggerUser, GETUTCDATE())";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId, TriggerUser = triggerUser });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Manual trigger created for job {JobId} by {User}", jobId, triggerUser);
        }

        return rowsAffected > 0;
    }

    public async Task<IEnumerable<JobTriggerViewModel>> GetPendingTriggersAsync(int jobId)
    {
        const string sql = @"
            SELECT
                TriggerId,
                JobId,
                TriggerType,
                TriggerUser,
                CreatedDate,
                ProcessedDate
            FROM [Service].[JobTriggers]
            WHERE JobId = @JobId AND ProcessedDate IS NULL
            ORDER BY CreatedDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<JobTriggerViewModel>(sql, new { JobId = jobId });
    }

    #endregion

    #region Service Status

    public async Task<ServiceStatusViewModel?> GetServiceStatusAsync()
    {
        const string sql = @"
            SELECT TOP 1
                ServiceName,
                MachineName,
                Status,
                LastHeartbeat,
                ServiceVersion,
                ActiveJobs,
                QueuedJobs
            FROM [Service].[ServiceStatus]
            ORDER BY LastHeartbeat DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ServiceStatusViewModel>(sql);
    }

    #endregion
}
