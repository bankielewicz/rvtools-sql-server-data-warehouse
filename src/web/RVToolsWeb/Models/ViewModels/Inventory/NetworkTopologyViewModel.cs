using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Network Topology report.
/// </summary>
public class NetworkTopologyViewModel
{
    public NetworkTopologyFilter Filter { get; set; } = new();
    public IEnumerable<NetworkTopologyItem> Items { get; set; } = Enumerable.Empty<NetworkTopologyItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Network Topology report.
/// </summary>
public class NetworkTopologyFilter
{
    public string? VI_SDK_Server { get; set; }
    public bool ShowOrphanedOnly { get; set; }
}

/// <summary>
/// Single port group record from the vw_Inventory_Network_Topology view.
/// </summary>
public class NetworkTopologyItem
{
    // Port Group Details
    public string? Port_Group { get; set; }
    public string? VLAN { get; set; }
    public string? Switch_Name { get; set; }
    public string? HostName { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? VI_SDK_Server { get; set; }

    // Switch Type
    public string? Switch_Type { get; set; }

    // Usage
    public int? VM_Count { get; set; }
    public bool Is_Orphaned { get; set; }

    // Security Settings
    public bool? Promiscuous_Mode { get; set; }
    public bool? Mac_Changes { get; set; }
    public bool? Forged_Transmits { get; set; }

    // Traffic Shaping
    public bool? Traffic_Shaping { get; set; }

    // Policy
    public string? Load_Balancing_Policy { get; set; }
}
