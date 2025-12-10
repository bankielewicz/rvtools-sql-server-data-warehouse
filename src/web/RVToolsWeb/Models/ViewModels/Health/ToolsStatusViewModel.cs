using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the VMware Tools Status report.
/// </summary>
public class ToolsStatusViewModel
{
    public ToolsStatusFilter Filter { get; set; } = new();
    public IEnumerable<ToolsStatusItem> Items { get; set; } = Enumerable.Empty<ToolsStatusItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int ToolsOkCount => Items.Count(x => x.ToolsStatus == "toolsOk");
    public int ToolsOldCount => Items.Count(x => x.ToolsStatus == "toolsOld");
    public int ToolsNotInstalledCount => Items.Count(x => x.ToolsStatus == "toolsNotInstalled");
    public int ToolsNotRunningCount => Items.Count(x => x.ToolsStatus == "toolsNotRunning");
    public int UpgradeableCount => Items.Count(x => x.Upgradeable == "1" || x.Upgradeable?.ToLower() == "true");
}

/// <summary>
/// Filter parameters for the Tools Status report.
/// </summary>
public class ToolsStatusFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single VM tools record from the vw_Tools_Status view.
/// </summary>
public class ToolsStatusItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? Powerstate { get; set; }
    public bool? Template { get; set; }
    public string? ToolsStatus { get; set; }
    public string? Tools_Version { get; set; }
    public string? Required_Version { get; set; }
    public string? Upgradeable { get; set; }
    public string? Upgrade_Policy { get; set; }
    public string? App_status { get; set; }
    public string? Heartbeat_status { get; set; }
    public string? Operation_Ready { get; set; }
    public string? State_change_support { get; set; }
    public string? Interactive_Guest { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Folder { get; set; }
    public string? OS_according_to_the_VMware_Tools { get; set; }
    public string? OS_according_to_the_configuration_file { get; set; }
    public string? VI_SDK_Server { get; set; }
}
