using RVToolsShared.Models;

namespace RVToolsService.Services;

/// <summary>
/// Core service for orchestrating RVTools import jobs.
/// Handles end-to-end import: Excel reading → Staging → MERGE → File movement.
/// </summary>
public interface IImportJobService
{
    /// <summary>
    /// Executes an import for a single file.
    /// </summary>
    /// <param name="filePath">Full path to the xlsx file</param>
    /// <param name="job">Job configuration</param>
    /// <param name="triggerType">How the import was triggered</param>
    /// <param name="triggerUser">Username if manually triggered</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result</returns>
    Task<ImportResult> ExecuteImportAsync(
        string filePath,
        JobDto job,
        string triggerType,
        string? triggerUser = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes imports for all files in a job's incoming folder.
    /// </summary>
    /// <param name="job">Job configuration</param>
    /// <param name="triggerType">How the import was triggered</param>
    /// <param name="triggerUser">Username if manually triggered</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job run result</returns>
    Task<JobRunResult> ExecuteJobAsync(
        JobDto job,
        string triggerType,
        string? triggerUser = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of importing a single file.
/// </summary>
public class ImportResult
{
    public int ImportBatchId { get; set; }
    public string Status { get; set; } = "Success";
    public string FileName { get; set; } = string.Empty;
    public int SheetsProcessed { get; set; }
    public int TotalSourceRows { get; set; }
    public int TotalStagedRows { get; set; }
    public int TotalFailedRows { get; set; }
    public int DurationMs { get; set; }
    public string? DestinationPath { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of executing a job (processing multiple files).
/// </summary>
public class JobRunResult
{
    public long JobRunId { get; set; }
    public string Status { get; set; } = "Success";
    public int FilesProcessed { get; set; }
    public int FilesFailed { get; set; }
    public int TotalRowsStaged { get; set; }
    public int TotalRowsFailed { get; set; }
    public int DurationMs { get; set; }
    public List<ImportResult> FileResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
