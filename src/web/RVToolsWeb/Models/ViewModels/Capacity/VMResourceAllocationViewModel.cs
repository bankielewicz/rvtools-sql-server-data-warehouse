using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Capacity;

/// <summary>
/// View model for the VM Resource Allocation report.
/// </summary>
public class VMResourceAllocationViewModel
{
    public VMResourceAllocationFilter Filter { get; set; } = new();
    public IEnumerable<VMResourceAllocationItem> Items { get; set; } = Enumerable.Empty<VMResourceAllocationItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Datacenters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Powerstates { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalVMs => Items.Count();
    public int PoweredOnCount => Items.Count(x => x.Powerstate?.ToLower() == "poweredon");
    public int TotalCPUs => Items.Sum(x => x.CPU_Count ?? 0);
    public long TotalMemoryGiB => Items.Sum(x => x.Memory_Size_MiB ?? 0) / 1024;
    public int CPUHotAddEnabledCount => Items.Count(x => x.CPU_Hot_Add?.ToLower() == "true");
    public int MemoryHotAddEnabledCount => Items.Count(x => x.Memory_Hot_Add?.ToLower() == "true");
}

/// <summary>
/// Filter parameters for the VM Resource Allocation report.
/// </summary>
public class VMResourceAllocationFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Powerstate { get; set; }
}

/// <summary>
/// Single VM record from the vw_VM_Resource_Allocation view.
/// </summary>
public class VMResourceAllocationItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Powerstate { get; set; }
    public bool? Template { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }

    // CPU Allocation
    public int? CPU_Count { get; set; }
    public int? CPU_Sockets { get; set; }
    public int? Cores_Per_Socket { get; set; }
    public int? CPU_Overall_MHz { get; set; }
    public int? CPU_Reservation_MHz { get; set; }
    public int? CPU_Limit_MHz { get; set; }
    public string? CPU_Shares_Level { get; set; }
    public string? CPU_Hot_Add { get; set; }

    // Memory Allocation
    public long? Memory_Size_MiB { get; set; }
    public long? Memory_Consumed_MiB { get; set; }
    public long? Memory_Active_MiB { get; set; }
    public long? Memory_Ballooned_MiB { get; set; }
    public long? Memory_Swapped_MiB { get; set; }
    public long? Memory_Reservation_MiB { get; set; }
    public long? Memory_Limit_MiB { get; set; }
    public string? Memory_Shares_Level { get; set; }
    public string? Memory_Hot_Add { get; set; }

    public string? OS_according_to_the_VMware_Tools { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
