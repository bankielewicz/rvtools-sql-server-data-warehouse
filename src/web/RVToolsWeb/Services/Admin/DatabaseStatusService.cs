using Dapper;
using Microsoft.Data.SqlClient;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service for retrieving database health and status information.
/// </summary>
public class DatabaseStatusService : IDatabaseStatusService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseStatusService> _logger;

    public DatabaseStatusService(ISqlConnectionFactory connectionFactory, ILogger<DatabaseStatusService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<DatabaseStatusViewModel> GetStatusAsync()
    {
        var status = new DatabaseStatusViewModel { LastChecked = DateTime.UtcNow };

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await ((SqlConnection)connection).OpenAsync();

            status.IsConnected = true;
            status.ServerVersion = ((SqlConnection)connection).ServerVersion;
            status.DatabaseName = connection.Database;

            // Get last import batch
            const string batchSql = @"
                SELECT TOP 1
                    ImportBatchId,
                    SourceFile,
                    VIServer,
                    ImportEndTime,
                    Status,
                    TotalRowsMerged,
                    TotalRowsFailed
                FROM [Audit].[ImportBatch]
                ORDER BY ImportBatchId DESC";

            var batch = await connection.QuerySingleOrDefaultAsync<dynamic>(batchSql);
            if (batch != null)
            {
                status.LastImportBatchId = batch.ImportBatchId;
                status.LastImportFile = batch.SourceFile;
                status.LastImportVIServer = batch.VIServer;
                status.LastImportDate = batch.ImportEndTime;
                status.LastImportStatus = batch.Status;
                status.LastImportRowsMerged = batch.TotalRowsMerged;
                status.LastImportRowsFailed = batch.TotalRowsFailed;
            }

            // Get record counts with safety checks for missing tables
            const string countsSql = @"
                SELECT
                    (SELECT ISNULL(COUNT(*), 0) FROM [Current].[vInfo]) AS TotalVMs,
                    (SELECT ISNULL(COUNT(*), 0) FROM [Current].[vHost]) AS TotalHosts,
                    (SELECT ISNULL(COUNT(*), 0) FROM [Current].[vDatastore]) AS TotalDatastores,
                    (SELECT ISNULL(COUNT(*), 0) FROM [Current].[vCluster]) AS TotalClusters,
                    (SELECT ISNULL(COUNT(*), 0) FROM [Audit].[ImportBatch]) AS TotalImportBatches";

            var counts = await connection.QuerySingleAsync<dynamic>(countsSql);
            status.TotalVMs = counts.TotalVMs;
            status.TotalHosts = counts.TotalHosts;
            status.TotalDatastores = counts.TotalDatastores;
            status.TotalClusters = counts.TotalClusters;
            status.TotalImportBatches = counts.TotalImportBatches;

            // Get schema table counts
            const string schemaSql = @"
                SELECT
                    TABLE_SCHEMA AS SchemaName,
                    COUNT(*) AS TableCount
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA IN ('Staging', 'Current', 'History')
                  AND TABLE_TYPE = 'BASE TABLE'
                GROUP BY TABLE_SCHEMA";

            var schemas = await connection.QueryAsync<dynamic>(schemaSql);
            foreach (var schema in schemas)
            {
                switch ((string)schema.SchemaName)
                {
                    case "Staging":
                        status.StagingTableCount = schema.TableCount;
                        break;
                    case "Current":
                        status.CurrentTableCount = schema.TableCount;
                        break;
                    case "History":
                        status.HistoryTableCount = schema.TableCount;
                        break;
                }
            }

            // Get total history records (approximation using sys.dm_db_partition_stats for performance)
            const string historyCountSql = @"
                SELECT ISNULL(SUM(p.rows), 0) AS TotalRows
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                INNER JOIN sys.dm_db_partition_stats p ON t.object_id = p.object_id
                WHERE s.name = 'History'
                  AND p.index_id IN (0, 1)";

            status.TotalHistoryRecords = await connection.QuerySingleAsync<long>(historyCountSql);
        }
        catch (Exception ex)
        {
            status.IsConnected = false;
            status.ConnectionError = ex.Message;
            _logger.LogError(ex, "Failed to get database status");
        }

        return status;
    }

    public async Task<IEnumerable<TableMappingViewModel>> GetTableMappingsAsync()
    {
        const string sql = @"
            SELECT
                tm.TableName,
                tm.NaturalKeyColumns,
                tm.IsActive,
                tm.CreatedDate,
                tm.ModifiedDate,
                (SELECT COUNT(*) FROM [Config].[ColumnMapping] cm WHERE cm.TableName = tm.TableName) AS ColumnCount
            FROM [Config].[TableMapping] tm
            ORDER BY tm.TableName";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<TableMappingViewModel>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get table mappings");
            return Enumerable.Empty<TableMappingViewModel>();
        }
    }

    public async Task<int> GetColumnMappingCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM [Config].[ColumnMapping]";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get column mapping count");
            return 0;
        }
    }
}
