using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Resource Pool Utilization report.
/// </summary>
public class ResourcePoolViewModel
{
    public ResourcePoolFilter Filter { get; set; } = new();
    public IEnumerable<ResourcePoolItem> Items { get; set; } = Enumerable.Empty<ResourcePoolItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Resource Pool Utilization report.
/// </summary>
public class ResourcePoolFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single resource pool record from the vw_MultiVCenter_ResourcePool_Utilization view.
/// </summary>
public class ResourcePoolItem
{
    // Resource Pool Identity
    public string ResourcePool { get; set; } = string.Empty;
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }

    // VM Counts
    public int? VMs_PoweredOn { get; set; }
    public int? Total_VMs { get; set; }

    // CPU Allocation
    public long? Total_vCPUs { get; set; }
    public long? Total_CPU_Reservation_MHz { get; set; }
    public long? Total_CPU_Limit_MHz { get; set; }

    // Memory Allocation
    public long? Total_Memory_MiB { get; set; }
    public long? Total_Active_Memory_MiB { get; set; }
    public long? Total_Memory_Reservation_MiB { get; set; }
    public long? Total_Memory_Limit_MiB { get; set; }

    // Average Active Memory Percent
    public decimal? Avg_Memory_Active_Percent { get; set; }
}
