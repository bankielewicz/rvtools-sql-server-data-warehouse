using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the VM Inventory report.
/// </summary>
public class VMInventoryViewModel
{
    /// <summary>
    /// Current filter values applied to the report.
    /// </summary>
    public VMInventoryFilter Filter { get; set; } = new();

    /// <summary>
    /// List of VMs matching the filter criteria.
    /// </summary>
    public IEnumerable<VMInventoryItem> Items { get; set; } = Enumerable.Empty<VMInventoryItem>();

    /// <summary>
    /// Available datacenters for the filter dropdown.
    /// </summary>
    public IEnumerable<FilterOptionDto> Datacenters { get; set; } = Enumerable.Empty<FilterOptionDto>();

    /// <summary>
    /// Available clusters for the filter dropdown (cascades from datacenter).
    /// </summary>
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();

    /// <summary>
    /// Available VI SDK Servers (vCenters) for the filter dropdown.
    /// </summary>
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    /// <summary>
    /// Available powerstates for the filter dropdown.
    /// </summary>
    public IEnumerable<FilterOptionDto> Powerstates { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the VM Inventory report.
/// </summary>
public class VMInventoryFilter
{
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Powerstate { get; set; }
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single VM record from the vw_VM_Inventory view.
/// </summary>
public class VMInventoryItem
{
    // Identity
    public string VM { get; set; } = string.Empty;
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }

    // State
    public string? Powerstate { get; set; }
    public bool? Template { get; set; }
    public string? Config_status { get; set; }
    public string? Guest_state { get; set; }

    // Resources
    public int? CPUs { get; set; }
    public int? Memory { get; set; }
    public int? NICs { get; set; }
    public int? Disks { get; set; }

    // Storage (MiB)
    public decimal? Total_disk_capacity_MiB { get; set; }
    public decimal? Provisioned_MiB { get; set; }
    public decimal? In_Use_MiB { get; set; }

    // Network
    public string? Primary_IP_Address { get; set; }
    public string? DNS_Name { get; set; }

    // Location
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Folder { get; set; }
    public string? Resource_pool { get; set; }

    // OS
    public string? OS_according_to_the_VMware_Tools { get; set; }
    public string? OS_according_to_the_configuration_file { get; set; }

    // Hardware
    public int? HW_version { get; set; }
    public string? Firmware { get; set; }

    // Dates
    public DateTime? Creation_date { get; set; }
    public DateTime? PowerOn { get; set; }

    // Metadata
    public string? Annotation { get; set; }
    public string? Path { get; set; }
}
