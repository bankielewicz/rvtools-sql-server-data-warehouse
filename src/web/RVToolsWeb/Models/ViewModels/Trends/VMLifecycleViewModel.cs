using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the VM Lifecycle report.
/// </summary>
public class VMLifecycleViewModel
{
    public VMLifecycleFilter Filter { get; set; } = new();
    public IEnumerable<VMLifecycleItem> Items { get; set; } = Enumerable.Empty<VMLifecycleItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalRecords => Items.Count();
    public int UniqueVMCount => Items.Select(x => x.VM_UUID).Distinct().Count();
    public int PoweredOnCount => Items.Count(x => x.Powerstate?.ToLower() == "poweredon");
    public int PoweredOffCount => Items.Count(x => x.Powerstate?.ToLower() == "poweredoff");
    public int TotalDaysTracked => Items.Sum(x => x.Days_In_State);
}

/// <summary>
/// Filter parameters for the VM Lifecycle report.
/// </summary>
public class VMLifecycleFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? VMName { get; set; }
    public string? Powerstate { get; set; }
    public int LookbackDays { get; set; } = 90;
}

/// <summary>
/// Single lifecycle record from the vw_Trends_VM_Lifecycle view.
/// </summary>
public class VMLifecycleItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Resource_pool { get; set; }
    public string? Powerstate { get; set; }
    public DateTime State_Start_Date { get; set; }
    public DateTime State_End_Date { get; set; }
    public int Days_In_State { get; set; }
    public DateTime? Last_PowerOn_Time { get; set; }
    public bool? Template { get; set; }
    public string? OS_according_to_the_VMware_Tools { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
