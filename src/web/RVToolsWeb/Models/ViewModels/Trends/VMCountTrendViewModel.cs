using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Shared;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the VM Count Trend report.
/// </summary>
public class VMCountTrendViewModel
{
    public VMCountTrendFilter Filter { get; set; } = new();
    public IEnumerable<VMCountTrendItem> Items { get; set; } = Enumerable.Empty<VMCountTrendItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Chart data
    public ChartDataViewModel ChartData { get; set; } = new();
    public ChartOptions ChartOptions { get; set; } = new()
    {
        ChartType = "line",
        Title = "VM Count Over Time",
        YAxisLabel = "Count",
        XAxisLabel = "Date",
        BeginAtZero = true,
        ShowLegend = true,
        Height = 350
    };

    // Summary metrics (latest snapshot)
    public int LatestVMCount => Items.OrderByDescending(x => x.SnapshotDate).FirstOrDefault()?.VMCount ?? 0;
    public int LatestPoweredOnCount => Items.OrderByDescending(x => x.SnapshotDate).FirstOrDefault()?.PoweredOnCount ?? 0;
    public int LatestPoweredOffCount => Items.OrderByDescending(x => x.SnapshotDate).FirstOrDefault()?.PoweredOffCount ?? 0;
    public int DataPointCount => Items.Select(x => x.SnapshotDate).Distinct().Count();

    // Growth calculation
    public int VMCountChange
    {
        get
        {
            var ordered = Items.OrderBy(x => x.SnapshotDate).ToList();
            if (ordered.Count < 2) return 0;
            var first = ordered.First().VMCount;
            var last = ordered.Last().VMCount;
            return last - first;
        }
    }
}

/// <summary>
/// Filter parameters for the VM Count Trend report.
/// </summary>
public class VMCountTrendFilter : DateRangeFilter
{
    public string? VI_SDK_Server { get; set; }

    protected override int DefaultDaysBack => 30;
}

/// <summary>
/// Single data point from the vw_VM_Count_Trend view.
/// </summary>
public class VMCountTrendItem
{
    public DateTime SnapshotDate { get; set; }
    public string? VI_SDK_Server { get; set; }
    public int VMCount { get; set; }
    public int TemplateCount { get; set; }
    public int PoweredOnCount { get; set; }
    public int PoweredOffCount { get; set; }
    public int SuspendedCount { get; set; }
    public int? ImportBatchId { get; set; }
}
