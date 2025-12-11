using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service for managing Config.TableRetention database table.
/// </summary>
public class TableRetentionService : ITableRetentionService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<TableRetentionService> _logger;

    public TableRetentionService(ISqlConnectionFactory connectionFactory, ILogger<TableRetentionService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<TableRetentionViewModel>> GetAllRetentionsAsync()
    {
        const string sql = @"
            SELECT
                TableRetentionId,
                SchemaName,
                TableName,
                RetentionDays,
                CreatedDate,
                ModifiedDate
            FROM [Config].[TableRetention]
            ORDER BY SchemaName, TableName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TableRetentionViewModel>(sql);
    }

    public async Task<IEnumerable<string>> GetAvailableTablesAsync()
    {
        // Get History tables that don't already have a retention override
        const string sql = @"
            SELECT t.TABLE_SCHEMA + '.' + t.TABLE_NAME AS FullName
            FROM INFORMATION_SCHEMA.TABLES t
            WHERE t.TABLE_SCHEMA = 'History'
              AND t.TABLE_TYPE = 'BASE TABLE'
              AND NOT EXISTS (
                  SELECT 1 FROM [Config].[TableRetention] tr
                  WHERE tr.SchemaName = t.TABLE_SCHEMA
                    AND tr.TableName = t.TABLE_NAME
              )
            ORDER BY t.TABLE_NAME";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql);
    }

    public async Task<bool> AddRetentionAsync(string schemaName, string tableName, int retentionDays)
    {
        const string sql = @"
            INSERT INTO [Config].[TableRetention] (SchemaName, TableName, RetentionDays, CreatedDate, ModifiedDate)
            VALUES (@SchemaName, @TableName, @RetentionDays, GETUTCDATE(), GETUTCDATE())";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                SchemaName = schemaName,
                TableName = tableName,
                RetentionDays = retentionDays
            });

            if (rowsAffected > 0)
            {
                _logger.LogInformation("Added retention override: {Schema}.{Table} = {Days} days",
                    schemaName, tableName, retentionDays);
            }

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add retention override for {Schema}.{Table}", schemaName, tableName);
            return false;
        }
    }

    public async Task<bool> UpdateRetentionAsync(int id, int retentionDays)
    {
        const string sql = @"
            UPDATE [Config].[TableRetention]
            SET RetentionDays = @RetentionDays,
                ModifiedDate = GETUTCDATE()
            WHERE TableRetentionId = @Id";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, RetentionDays = retentionDays });

            if (rowsAffected > 0)
            {
                _logger.LogInformation("Updated retention override ID {Id} to {Days} days", id, retentionDays);
            }

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update retention override ID {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteRetentionAsync(int id)
    {
        const string sql = @"DELETE FROM [Config].[TableRetention] WHERE TableRetentionId = @Id";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

            if (rowsAffected > 0)
            {
                _logger.LogInformation("Deleted retention override ID {Id}", id);
            }

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete retention override ID {Id}", id);
            return false;
        }
    }
}
