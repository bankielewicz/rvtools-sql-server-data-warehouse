using RVToolsWeb.Models.DTOs;
using RVToolsWeb.Models.ViewModels.Shared;

namespace RVToolsWeb.Models.ViewModels.Trends;

/// <summary>
/// View model for the Host Utilization Trend report.
/// </summary>
public class HostUtilizationViewModel
{
    public HostUtilizationFilter Filter { get; set; } = new();
    public IEnumerable<HostUtilizationItem> Items { get; set; } = Enumerable.Empty<HostUtilizationItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Hosts { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Chart data
    public ChartDataViewModel ChartData { get; set; } = new();
    public ChartOptions ChartOptions { get; set; } = new()
    {
        ChartType = "line",
        Title = "Host Utilization Over Time",
        YAxisLabel = "Percentage",
        XAxisLabel = "Date",
        BeginAtZero = true,
        ShowLegend = true,
        Height = 350
    };

    // Summary metrics
    public int DataPointCount => Items.Select(x => x.SnapshotDate).Distinct().Count();
    public int UniqueHostCount => Items.Select(x => x.HostName).Distinct().Count();
    public decimal AvgCPUPercent => Items.Any() && Items.Any(x => x.CPU_Usage_Percent.HasValue)
        ? Items.Where(x => x.CPU_Usage_Percent.HasValue).Average(x => x.CPU_Usage_Percent!.Value)
        : 0;
    public decimal AvgMemoryPercent => Items.Any() && Items.Any(x => x.Memory_Usage_Percent.HasValue)
        ? Items.Where(x => x.Memory_Usage_Percent.HasValue).Average(x => x.Memory_Usage_Percent!.Value)
        : 0;
    public int TotalVMs => Items.Any()
        ? Items.OrderByDescending(x => x.SnapshotDate).GroupBy(x => x.HostName).Sum(g => g.First().VM_Count ?? 0)
        : 0;
}

/// <summary>
/// Filter parameters for the Host Utilization report.
/// </summary>
public class HostUtilizationFilter : DateRangeFilter
{
    public string? VI_SDK_Server { get; set; }
    public string? HostName { get; set; }
    public string? Cluster { get; set; }

    protected override int DefaultDaysBack => 30;
}

/// <summary>
/// Single data point from the vw_Trends_Host_Utilization view.
/// </summary>
public class HostUtilizationItem
{
    public DateTime SnapshotDate { get; set; }
    public string? HostName { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public decimal? CPU_Usage_Percent { get; set; }
    public int? Physical_CPUs { get; set; }
    public int? Cores_per_CPU { get; set; }
    public int? Total_Cores { get; set; }
    public int? Total_vCPUs { get; set; }
    public decimal? vCPU_to_Core_Ratio { get; set; }
    public decimal? Memory_Usage_Percent { get; set; }
    public long? Physical_Memory_MiB { get; set; }
    public long? Allocated_vMemory_MiB { get; set; }
    public int? VM_Count { get; set; }
    public decimal? VMs_per_Core { get; set; }
    public bool? in_Maintenance_Mode { get; set; }
    public string? ESX_Version { get; set; }
    public int? ImportBatchId { get; set; }
}
