using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the Snapshot Aging report.
/// </summary>
public class SnapshotAgingViewModel
{
    public SnapshotAgingFilter Filter { get; set; } = new();
    public IEnumerable<SnapshotAgingItem> Items { get; set; } = Enumerable.Empty<SnapshotAgingItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public decimal TotalSnapshotSizeGiB => Items.Sum(x => (x.Size_MiB_total ?? 0) / 1024m);
    public int OldSnapshotCount => Items.Count(x => x.AgeDays >= 7);
}

/// <summary>
/// Filter parameters for the Snapshot Aging report.
/// </summary>
public class SnapshotAgingFilter
{
    public string? VI_SDK_Server { get; set; }
    public int MinAgeDays { get; set; } = 7;
}

/// <summary>
/// Single snapshot record from the vw_Snapshot_Aging view.
/// </summary>
public class SnapshotAgingItem
{
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? Powerstate { get; set; }
    public string? SnapshotName { get; set; }
    public string? Description { get; set; }
    public DateTime? SnapshotDate { get; set; }
    public string? Filename { get; set; }
    public decimal? Size_MiB_vmsn { get; set; }
    public decimal? Size_MiB_total { get; set; }
    public int? AgeDays { get; set; }
    public bool? Quiesced { get; set; }
    public string? State { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Folder { get; set; }
    public string? OS_according_to_the_VMware_Tools { get; set; }
    public string? VI_SDK_Server { get; set; }
}
