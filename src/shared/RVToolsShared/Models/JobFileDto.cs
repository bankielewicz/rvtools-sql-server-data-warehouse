namespace RVToolsShared.Models;

/// <summary>
/// DTO representing a file discovered in the incoming folder for processing.
/// </summary>
public class JobFileDto
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Parsed export date from filename (historical imports).
    /// Null if date could not be parsed or file uses standard naming.
    /// </summary>
    public DateTime? ParsedExportDate { get; set; }

    /// <summary>
    /// Parsed vCenter/VIServer name from filename.
    /// </summary>
    public string? ParsedVIServer { get; set; }
}

/// <summary>
/// Result of processing a single file.
/// </summary>
public class FileProcessingResult
{
    public string FilePath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int? ImportBatchId { get; set; }
    public int RowsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}
