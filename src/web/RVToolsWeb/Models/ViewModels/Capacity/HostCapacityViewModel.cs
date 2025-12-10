using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Capacity;

/// <summary>
/// View model for the Host Capacity report.
/// </summary>
public class HostCapacityViewModel
{
    public HostCapacityFilter Filter { get; set; } = new();
    public IEnumerable<HostCapacityItem> Items { get; set; } = Enumerable.Empty<HostCapacityItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Datacenters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalHosts => Items.Count();
    public int CPUCriticalCount => Items.Count(x => x.CPUStatus == "Critical");
    public int CPUWarningCount => Items.Count(x => x.CPUStatus == "Warning");
    public int MemoryCriticalCount => Items.Count(x => x.MemoryStatus == "Critical");
    public int MemoryWarningCount => Items.Count(x => x.MemoryStatus == "Warning");
    public int MaintenanceModeCount => Items.Count(x => x.in_Maintenance_Mode == true);
    public int TotalVMs => Items.Sum(x => x.Num_VMs ?? 0);
}

/// <summary>
/// Filter parameters for the Host Capacity report.
/// </summary>
public class HostCapacityFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
}

/// <summary>
/// Single host record from the vw_Host_Capacity view.
/// </summary>
public class HostCapacityItem
{
    public string? Host { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public int? Num_Cores { get; set; }
    public decimal? CPU_Usage_Percent { get; set; }
    public string? CPUStatus { get; set; }
    public int? Memory_MB { get; set; }
    public decimal? Memory_Usage_Percent { get; set; }
    public string? MemoryStatus { get; set; }
    public int? Num_VMs { get; set; }
    public int? Num_vCPUs { get; set; }
    public decimal? vCPUs_per_Core { get; set; }
    public long? vRAM_MB { get; set; }
    public bool? in_Maintenance_Mode { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
