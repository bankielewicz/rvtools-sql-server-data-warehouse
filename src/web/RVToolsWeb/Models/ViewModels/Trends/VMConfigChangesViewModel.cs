using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Shared;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the VM Configuration Changes report.
/// </summary>
public class VMConfigChangesViewModel
{
    public VMConfigChangesFilter Filter { get; set; } = new();
    public IEnumerable<VMConfigChangesItem> Items { get; set; } = Enumerable.Empty<VMConfigChangesItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalChanges => Items.Count();
    public int UniqueVMCount => Items.Select(x => x.VM_UUID).Distinct().Count();
    public int CPUChanges => Items.Count(x => x.CPUs.HasValue);
    public int MemoryChanges => Items.Count(x => x.Memory_MB.HasValue);
    public int PowerstateChanges => Items.Count(x => !string.IsNullOrEmpty(x.Powerstate));
}

/// <summary>
/// Filter parameters for the VM Config Changes report.
/// </summary>
public class VMConfigChangesFilter : DateRangeFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? VMName { get; set; }

    protected override int DefaultDaysBack => 30;
}

/// <summary>
/// Single change record from the vw_VM_Config_Changes view.
/// </summary>
public class VMConfigChangesItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveUntil { get; set; }
    public DateTime? ChangedDate { get; set; }
    public string? Powerstate { get; set; }
    public int? CPUs { get; set; }
    public long? Memory_MB { get; set; }
    public int? NICs { get; set; }
    public int? Disks { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? HW_version { get; set; }
    public string? OS_according_to_the_VMware_Tools { get; set; }
    public string? SourceFile { get; set; }
    public int? ImportBatchId { get; set; }
}
