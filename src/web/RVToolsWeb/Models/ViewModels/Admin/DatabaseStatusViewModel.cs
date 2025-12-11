namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Database connection health and import statistics.
/// </summary>
public class DatabaseStatusViewModel
{
    // Connection Health
    public bool IsConnected { get; set; }
    public string? ConnectionError { get; set; }
    public string ServerVersion { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public DateTime? LastChecked { get; set; }

    // Last Import Batch
    public int? LastImportBatchId { get; set; }
    public string? LastImportFile { get; set; }
    public string? LastImportVIServer { get; set; }
    public DateTime? LastImportDate { get; set; }
    public string? LastImportStatus { get; set; }
    public int? LastImportRowsMerged { get; set; }
    public int? LastImportRowsFailed { get; set; }

    // Record Counts
    public int TotalVMs { get; set; }
    public int TotalHosts { get; set; }
    public int TotalDatastores { get; set; }
    public int TotalClusters { get; set; }
    public int TotalImportBatches { get; set; }
    public long TotalHistoryRecords { get; set; }

    // Schema Info
    public int StagingTableCount { get; set; }
    public int CurrentTableCount { get; set; }
    public int HistoryTableCount { get; set; }
}
