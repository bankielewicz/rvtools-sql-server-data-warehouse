using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Shared;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the Storage Growth report.
/// </summary>
public class StorageGrowthViewModel
{
    public StorageGrowthFilter Filter { get; set; } = new();
    public IEnumerable<StorageGrowthItem> Items { get; set; } = Enumerable.Empty<StorageGrowthItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Datastores { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Chart data
    public ChartDataViewModel ChartData { get; set; } = new();
    public ChartOptions ChartOptions { get; set; } = new()
    {
        ChartType = "line",
        Title = "Storage Usage Over Time",
        YAxisLabel = "Used (GiB)",
        XAxisLabel = "Date",
        BeginAtZero = false,
        ShowLegend = true,
        Height = 350
    };

    // Summary metrics (latest snapshot)
    public long LatestCapacityGiB => Items.Any()
        ? Items.OrderByDescending(x => x.SnapshotDate).First().Capacity_MiB / 1024
        : 0;
    public long LatestUsedGiB => Items.Any()
        ? Items.OrderByDescending(x => x.SnapshotDate).First().In_Use_MiB / 1024
        : 0;
    public long LatestFreeGiB => Items.Any()
        ? Items.OrderByDescending(x => x.SnapshotDate).First().Free_MiB / 1024
        : 0;
    public decimal LatestFreePercent => Items.Any()
        ? Items.OrderByDescending(x => x.SnapshotDate).First().Free_Percent ?? 0
        : 0;
    public int DataPointCount => Items.Select(x => x.SnapshotDate).Distinct().Count();

    // Growth calculation (GiB change over time period)
    public long UsedGrowthGiB
    {
        get
        {
            var ordered = Items.OrderBy(x => x.SnapshotDate).ToList();
            if (ordered.Count < 2) return 0;
            var first = ordered.First().In_Use_MiB;
            var last = ordered.Last().In_Use_MiB;
            return (last - first) / 1024;
        }
    }
}

/// <summary>
/// Filter parameters for the Storage Growth report.
/// </summary>
public class StorageGrowthFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? DatastoreName { get; set; }
    public int LookbackDays { get; set; } = 30;
}

/// <summary>
/// Single data point from the vw_Trends_Storage_Growth view.
/// </summary>
public class StorageGrowthItem
{
    public DateTime SnapshotDate { get; set; }
    public string? DatastoreName { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Type { get; set; }
    public long Capacity_MiB { get; set; }
    public long In_Use_MiB { get; set; }
    public long Free_MiB { get; set; }
    public decimal? Free_Percent { get; set; }
    public int DayNumber { get; set; }
    public int? ImportBatchId { get; set; }
}
