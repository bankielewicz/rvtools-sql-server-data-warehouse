using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Capacity;

/// <summary>
/// View model for the VM Right-Sizing report.
/// </summary>
public class VMRightSizingViewModel
{
    public VMRightSizingFilter Filter { get; set; } = new();
    public IEnumerable<VMRightSizingItem> Items { get; set; } = Enumerable.Empty<VMRightSizingItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Datacenters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalVMs => Items.Count();
    public int UnderutilizedMemoryCount => Items.Count(x => x.Memory_Active_Percent.HasValue && x.Memory_Active_Percent < 50);
    public int HighlyUnderutilizedCount => Items.Count(x => x.Memory_Active_Percent.HasValue && x.Memory_Active_Percent < 25);
    public long TotalAllocatedMemoryGiB => Items.Sum(x => x.Memory_Allocated_MiB ?? 0) / 1024;
    public long TotalActiveMemoryGiB => Items.Sum(x => x.Memory_Active_MiB ?? 0) / 1024;
    public int BalloonedCount => Items.Count(x => x.Memory_Ballooned_MiB > 0);
}

/// <summary>
/// Filter parameters for the VM Right-Sizing report.
/// </summary>
public class VMRightSizingFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public int? MaxActivePercent { get; set; } = 50; // Default: Show VMs using less than 50% of memory
}

/// <summary>
/// Single VM record from the vw_Capacity_VM_RightSizing view.
/// </summary>
public class VMRightSizingItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Powerstate { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Resource_pool { get; set; }

    // CPU Metrics
    public int? CPU_Allocated { get; set; }
    public decimal? CPU_Readiness_Percent { get; set; }
    public int? CPU_Reservation_MHz { get; set; }

    // Memory Metrics
    public long? Memory_Allocated_MiB { get; set; }
    public long? Memory_Active_MiB { get; set; }
    public long? Memory_Consumed_MiB { get; set; }
    public long? Memory_Ballooned_MiB { get; set; }
    public long? Memory_Reservation_MiB { get; set; }

    // Calculated Ratios
    public decimal? Memory_Active_Percent { get; set; }
    public decimal? Memory_Reservation_Percent { get; set; }

    public string? OS_according_to_the_VMware_Tools { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
