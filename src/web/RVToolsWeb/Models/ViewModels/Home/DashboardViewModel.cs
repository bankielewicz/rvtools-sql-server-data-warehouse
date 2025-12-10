namespace RVToolsWeb.Models.ViewModels.Home;

/// <summary>
/// View model for the Dashboard page with summary KPIs.
/// </summary>
public class DashboardViewModel
{
    // Infrastructure Summary
    public int TotalVMs { get; set; }
    public int VMsPoweredOn { get; set; }
    public int VMsPoweredOff { get; set; }
    public int Templates { get; set; }
    public int TotalHosts { get; set; }
    public int TotalClusters { get; set; }
    public int TotalDatastores { get; set; }
    public int TotalDatacenters { get; set; }
    public int TotalVCenters { get; set; }

    // Resource Summary
    public long TotalvCPUs { get; set; }
    public long TotalvMemoryGiB { get; set; }
    public long TotalStorageTiB { get; set; }
    public long UsedStorageTiB { get; set; }

    // Health Summary
    public int HealthIssueCount { get; set; }
    public int ExpiringCertificates { get; set; }
    public int AgingSnapshots { get; set; }
    public int OrphanedFiles { get; set; }

    // Capacity Alerts
    public int CriticalDatastores { get; set; }
    public int WarningDatastores { get; set; }

    // Last Import Info
    public DateTime? LastImportDate { get; set; }
    public int? LastImportBatchId { get; set; }

    // Calculated properties
    public decimal PoweredOnPercent => TotalVMs > 0 ? Math.Round((decimal)VMsPoweredOn / TotalVMs * 100, 1) : 0;
    public decimal StorageUsedPercent => TotalStorageTiB > 0 ? Math.Round((decimal)UsedStorageTiB / TotalStorageTiB * 100, 1) : 0;
}
