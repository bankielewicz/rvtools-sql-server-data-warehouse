namespace RVToolsService.Services;

/// <summary>
/// Service for managing Audit.ImportBatch records during imports.
/// </summary>
public interface IBatchService
{
    /// <summary>
    /// Creates a new import batch record.
    /// </summary>
    /// <param name="sourceFile">Name of the source xlsx file</param>
    /// <param name="viServer">Optional vCenter server identifier</param>
    /// <param name="exportDate">Optional RVTools export date (for historical imports)</param>
    /// <param name="jobRunId">Optional job run ID (from Service.JobRuns)</param>
    /// <returns>The new ImportBatchId</returns>
    Task<int> CreateBatchAsync(string sourceFile, string? viServer = null, DateTime? exportDate = null, long? jobRunId = null);

    /// <summary>
    /// Updates an import batch with final statistics.
    /// </summary>
    Task UpdateBatchAsync(int importBatchId, string status, int totalSheets, int sheetsProcessed,
        int totalRowsSource, int totalRowsStaged, int totalRowsFailed, string? errorMessage = null);

    /// <summary>
    /// Marks a batch as failed with an error message.
    /// </summary>
    Task FailBatchAsync(int importBatchId, string errorMessage);
}
