using System.Reflection;
using Dapper;
using RVToolsShared.Data;

namespace RVToolsService.Services;

/// <summary>
/// Service for managing service health status and heartbeats in Service.ServiceStatus table.
/// </summary>
public class ServiceHealthService : IServiceHealthService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceHealthService> _logger;

    private readonly string _serviceName;
    private readonly string _machineName;
    private readonly string _serviceVersion;

    public ServiceHealthService(
        ISqlConnectionFactory connectionFactory,
        IConfiguration configuration,
        ILogger<ServiceHealthService> logger)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
        _logger = logger;

        _serviceName = configuration["ServiceSettings:ServiceName"] ?? "RVToolsImportService";
        _machineName = Environment.MachineName;
        _serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    /// <inheritdoc/>
    public async Task UpdateHeartbeatAsync(CancellationToken cancellationToken = default)
    {
        // Count active and queued jobs
        var (activeJobs, queuedJobs) = await GetJobCountsAsync();

        await UpsertStatusAsync("Running", activeJobs, queuedJobs, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var (activeJobs, queuedJobs) = status == "Running"
            ? await GetJobCountsAsync()
            : (0, 0);

        await UpsertStatusAsync(status, activeJobs, queuedJobs, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ServiceHealthInfo?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                ServiceName,
                MachineName,
                Status,
                LastHeartbeat,
                ServiceVersion,
                ActiveJobs,
                QueuedJobs
            FROM [Service].[ServiceStatus]
            WHERE ServiceName = @ServiceName AND MachineName = @MachineName";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<ServiceHealthInfo>(sql, new
        {
            ServiceName = _serviceName,
            MachineName = _machineName
        });
    }

    /// <summary>
    /// Inserts or updates the service status row.
    /// </summary>
    private async Task UpsertStatusAsync(string status, int activeJobs, int queuedJobs, CancellationToken cancellationToken)
    {
        const string sql = @"
            MERGE [Service].[ServiceStatus] AS target
            USING (SELECT @ServiceName AS ServiceName, @MachineName AS MachineName) AS source
            ON target.ServiceName = source.ServiceName AND target.MachineName = source.MachineName
            WHEN MATCHED THEN
                UPDATE SET
                    Status = @Status,
                    LastHeartbeat = GETUTCDATE(),
                    ServiceVersion = @ServiceVersion,
                    ActiveJobs = @ActiveJobs,
                    QueuedJobs = @QueuedJobs
            WHEN NOT MATCHED THEN
                INSERT (ServiceName, MachineName, Status, LastHeartbeat, ServiceVersion, ActiveJobs, QueuedJobs)
                VALUES (@ServiceName, @MachineName, @Status, GETUTCDATE(), @ServiceVersion, @ActiveJobs, @QueuedJobs);";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(sql, new
        {
            ServiceName = _serviceName,
            MachineName = _machineName,
            Status = status,
            ServiceVersion = _serviceVersion,
            ActiveJobs = activeJobs,
            QueuedJobs = queuedJobs
        });
    }

    /// <summary>
    /// Gets the count of active and queued jobs.
    /// </summary>
    private async Task<(int active, int queued)> GetJobCountsAsync()
    {
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM [Service].[JobRuns] WHERE Status = 'Running') AS ActiveJobs,
                (SELECT COUNT(*) FROM [Service].[JobTriggers] WHERE ProcessedDate IS NULL) AS QueuedJobs";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var result = await connection.QuerySingleAsync<(int ActiveJobs, int QueuedJobs)>(sql);
            return (result.ActiveJobs, result.QueuedJobs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get job counts: {Error}", ex.Message);
            return (0, 0);
        }
    }
}
