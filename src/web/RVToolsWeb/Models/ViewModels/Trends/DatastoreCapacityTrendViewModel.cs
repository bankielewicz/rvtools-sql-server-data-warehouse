using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Shared;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the Datastore Capacity Trend report.
/// </summary>
public class DatastoreCapacityTrendViewModel
{
    public DatastoreCapacityTrendFilter Filter { get; set; } = new();
    public IEnumerable<DatastoreCapacityTrendItem> Items { get; set; } = Enumerable.Empty<DatastoreCapacityTrendItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Datastores { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Chart data
    public ChartDataViewModel ChartData { get; set; } = new();
    public ChartOptions ChartOptions { get; set; } = new()
    {
        ChartType = "line",
        Title = "Datastore Capacity Trend",
        YAxisLabel = "Percentage",
        XAxisLabel = "Date",
        BeginAtZero = true,
        ShowLegend = true,
        Height = 350
    };

    // Summary metrics
    public int DataPointCount => Items.Select(x => x.SnapshotDate).Distinct().Count();
    public int UniqueDatastoreCount => Items.Select(x => x.DatastoreName).Distinct().Count();
    public decimal AvgFreePercent => Items.Any() && Items.Any(x => x.Free_Percent.HasValue)
        ? Items.Where(x => x.Free_Percent.HasValue).Average(x => x.Free_Percent!.Value)
        : 0;
    public int CriticalSnapshotCount => Items.Count(x => x.Free_Percent.HasValue && x.Free_Percent < 10);
}

/// <summary>
/// Filter parameters for the Datastore Capacity Trend report.
/// </summary>
public class DatastoreCapacityTrendFilter : DateRangeFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? DatastoreName { get; set; }

    protected override int DefaultDaysBack => 30;
}

/// <summary>
/// Single data point from the vw_Datastore_Capacity_Trend view.
/// </summary>
public class DatastoreCapacityTrendItem
{
    public DateTime SnapshotDate { get; set; }
    public string? DatastoreName { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Type { get; set; }
    public long Capacity_MiB { get; set; }
    public long Provisioned_MiB { get; set; }
    public long In_Use_MiB { get; set; }
    public long Free_MiB { get; set; }
    public decimal? Free_Percent { get; set; }
    public int? Num_VMs { get; set; }
    public int? ImportBatchId { get; set; }
}
