using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Datastore Inventory report.
/// </summary>
public class DatastoreInventoryViewModel
{
    public DatastoreInventoryFilter Filter { get; set; } = new();
    public IEnumerable<DatastoreInventoryItem> Items { get; set; } = Enumerable.Empty<DatastoreInventoryItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Datastore Inventory report.
/// </summary>
public class DatastoreInventoryFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single datastore record from the vw_Datastore_Inventory view.
/// </summary>
public class DatastoreInventoryItem
{
    // Identity
    public string DatastoreName { get; set; } = string.Empty;
    public string? VI_SDK_Server { get; set; }

    // Status
    public string? Config_status { get; set; }
    public bool? Accessible { get; set; }

    // Type
    public string? Type { get; set; }
    public string? Major_Version { get; set; }
    public string? Version { get; set; }
    public bool? VMFS_Upgradeable { get; set; }

    // Capacity (MiB)
    public decimal? Capacity_MiB { get; set; }
    public decimal? Provisioned_MiB { get; set; }
    public decimal? In_Use_MiB { get; set; }
    public decimal? Free_MiB { get; set; }
    public decimal? Free_Percent { get; set; }

    // Usage
    public int? Num_VMs { get; set; }
    public int? Num_Hosts { get; set; }

    // SIOC
    public bool? SIOC_enabled { get; set; }
    public int? SIOC_Threshold { get; set; }

    // Cluster
    public string? Cluster_name { get; set; }
    public decimal? Cluster_capacity_MiB { get; set; }
    public decimal? Cluster_free_space_MiB { get; set; }

    // Technical
    public int? Block_size { get; set; }
    public string? URL { get; set; }
}
