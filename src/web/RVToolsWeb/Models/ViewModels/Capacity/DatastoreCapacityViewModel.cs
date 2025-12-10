using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Capacity;

/// <summary>
/// View model for the Datastore Capacity report.
/// </summary>
public class DatastoreCapacityViewModel
{
    public DatastoreCapacityFilter Filter { get; set; } = new();
    public IEnumerable<DatastoreCapacityItem> Items { get; set; } = Enumerable.Empty<DatastoreCapacityItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int TotalDatastores => Items.Count();
    public int CriticalCount => Items.Count(x => x.CapacityStatus == "Critical");
    public int WarningCount => Items.Count(x => x.CapacityStatus == "Warning");
    public int NormalCount => Items.Count(x => x.CapacityStatus == "Normal");
    public long TotalCapacityTiB => Items.Sum(x => x.Capacity_MiB ?? 0) / 1024 / 1024;
    public long TotalFreeTiB => Items.Sum(x => x.Free_MiB ?? 0) / 1024 / 1024;
}

/// <summary>
/// Filter parameters for the Datastore Capacity report.
/// </summary>
public class DatastoreCapacityFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? StatusFilter { get; set; } // All, Critical, Warning
}

/// <summary>
/// Single datastore record from the vw_Datastore_Capacity view.
/// </summary>
public class DatastoreCapacityItem
{
    public string? DatastoreName { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Type { get; set; }
    public string? Cluster_name { get; set; }
    public long? Capacity_MiB { get; set; }
    public long? Provisioned_MiB { get; set; }
    public long? In_Use_MiB { get; set; }
    public long? Free_MiB { get; set; }
    public decimal? Free_Percent { get; set; }
    public decimal? OverProvisioningPercent { get; set; }
    public string? CapacityStatus { get; set; }
    public int? Num_VMs { get; set; }
    public int? Num_Hosts { get; set; }
    public string? Accessible { get; set; }
    public int? ImportBatchId { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
