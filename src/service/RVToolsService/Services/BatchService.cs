using Dapper;
using RVToolsShared.Data;

namespace RVToolsService.Services;

/// <summary>
/// Service for managing Audit.ImportBatch records during imports.
/// </summary>
public class BatchService : IBatchService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<BatchService> _logger;

    public BatchService(ISqlConnectionFactory connectionFactory, ILogger<BatchService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<int> CreateBatchAsync(string sourceFile, string? viServer = null, DateTime? exportDate = null, long? jobRunId = null)
    {
        const string sql = @"
            INSERT INTO [Audit].[ImportBatch] (SourceFile, VIServer, ImportStartTime, RVToolsExportDate, JobRunId, Status)
            OUTPUT INSERTED.ImportBatchId
            VALUES (@SourceFile, @VIServer, GETUTCDATE(), @ExportDate, @JobRunId, 'Running')";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var batchId = await connection.ExecuteScalarAsync<int>(sql, new
        {
            SourceFile = sourceFile,
            VIServer = viServer,
            ExportDate = exportDate ?? DateTime.UtcNow,
            JobRunId = jobRunId
        });

        _logger.LogInformation("Created import batch {BatchId} for file '{SourceFile}'", batchId, sourceFile);
        return batchId;
    }

    /// <inheritdoc/>
    public async Task UpdateBatchAsync(int importBatchId, string status, int totalSheets, int sheetsProcessed,
        int totalRowsSource, int totalRowsStaged, int totalRowsFailed, string? errorMessage = null)
    {
        const string sql = @"
            UPDATE [Audit].[ImportBatch]
            SET ImportEndTime = GETUTCDATE(),
                Status = @Status,
                TotalSheets = @TotalSheets,
                SheetsProcessed = @SheetsProcessed,
                TotalRowsSource = @TotalRowsSource,
                TotalRowsStaged = @TotalRowsStaged,
                TotalRowsFailed = @TotalRowsFailed,
                ErrorMessage = @ErrorMessage
            WHERE ImportBatchId = @ImportBatchId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new
        {
            ImportBatchId = importBatchId,
            Status = status,
            TotalSheets = totalSheets,
            SheetsProcessed = sheetsProcessed,
            TotalRowsSource = totalRowsSource,
            TotalRowsStaged = totalRowsStaged,
            TotalRowsFailed = totalRowsFailed,
            ErrorMessage = errorMessage
        });

        _logger.LogDebug("Updated import batch {BatchId}: Status={Status}, Staged={Staged}, Failed={Failed}",
            importBatchId, status, totalRowsStaged, totalRowsFailed);
    }

    /// <inheritdoc/>
    public async Task FailBatchAsync(int importBatchId, string errorMessage)
    {
        const string sql = @"
            UPDATE [Audit].[ImportBatch]
            SET ImportEndTime = GETUTCDATE(),
                Status = 'Failed',
                ErrorMessage = @ErrorMessage
            WHERE ImportBatchId = @ImportBatchId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new
        {
            ImportBatchId = importBatchId,
            ErrorMessage = errorMessage
        });

        _logger.LogWarning("Marked import batch {BatchId} as failed: {Error}", importBatchId, errorMessage);
    }
}
