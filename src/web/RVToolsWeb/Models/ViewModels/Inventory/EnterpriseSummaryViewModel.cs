using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Enterprise Summary report.
/// </summary>
public class EnterpriseSummaryViewModel
{
    public EnterpriseSummaryFilter Filter { get; set; } = new();
    public IEnumerable<EnterpriseSummaryItem> Items { get; set; } = Enumerable.Empty<EnterpriseSummaryItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Totals across all vCenters
    public int TotalVMsPoweredOn => Items.Sum(x => x.VMs_PoweredOn ?? 0);
    public int TotalVMsPoweredOff => Items.Sum(x => x.VMs_PoweredOff ?? 0);
    public int TotalTemplates => Items.Sum(x => x.Templates ?? 0);
    public int TotalVMs => Items.Sum(x => x.Total_VMs ?? 0);
    public long TotalvCPUs => Items.Sum(x => x.Total_vCPUs ?? 0);
    public long TotalvMemoryGiB => Items.Sum(x => (x.Total_vMemory_MiB ?? 0) / 1024);
    public long TotalProvisionedTiB => Items.Sum(x => (x.Total_Provisioned_MiB ?? 0) / 1024 / 1024);
    public int TotalClusters => Items.Sum(x => x.Cluster_Count ?? 0);
    public int TotalHosts => Items.Sum(x => x.Host_Count ?? 0);
    public int TotalDatacenters => Items.Sum(x => x.Datacenter_Count ?? 0);
}

/// <summary>
/// Filter parameters for the Enterprise Summary report.
/// </summary>
public class EnterpriseSummaryFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single vCenter summary record from the vw_MultiVCenter_Enterprise_Summary view.
/// </summary>
public class EnterpriseSummaryItem
{
    // vCenter Identity
    public string VI_SDK_Server { get; set; } = string.Empty;

    // VM Counts
    public int? VMs_PoweredOn { get; set; }
    public int? VMs_PoweredOff { get; set; }
    public int? Templates { get; set; }
    public int? Total_VMs { get; set; }

    // CPU Allocation
    public long? Total_vCPUs { get; set; }

    // Memory Allocation
    public long? Total_vMemory_MiB { get; set; }

    // Storage Allocation
    public long? Total_Provisioned_MiB { get; set; }
    public long? Total_InUse_MiB { get; set; }

    // Counts
    public int? Cluster_Count { get; set; }
    public int? Host_Count { get; set; }
    public int? Datacenter_Count { get; set; }

    // Audit
    public int? Latest_ImportBatchId { get; set; }
    public DateTime? Latest_Import_Date { get; set; }
}
