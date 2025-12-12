namespace RVToolsShared.Models;

/// <summary>
/// DTO representing an import batch from the Audit.ImportBatch table.
/// </summary>
public class ImportBatchDto
{
    public int ImportBatchId { get; set; }
    public long? JobRunId { get; set; }
    public string? SourceFile { get; set; }
    public DateTime ImportStartTime { get; set; }
    public DateTime? ImportEndTime { get; set; }
    public string Status { get; set; } = "Processing";
    public int? TotalRows { get; set; }
    public int? SuccessRows { get; set; }
    public int? FailedRows { get; set; }
    public string? VIServer { get; set; }
    public DateTime? RVToolsExportDate { get; set; }
}
