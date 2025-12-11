using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Shared;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving Host Utilization Trend report data.
/// </summary>
public class HostUtilizationService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public HostUtilizationService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HostUtilizationItem>> GetReportDataAsync(HostUtilizationFilter filter)
    {
        const string sql = @"
            SELECT
                SnapshotDate,
                HostName,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                CPU_Usage_Percent,
                Physical_CPUs,
                Cores_per_CPU,
                Total_Cores,
                Total_vCPUs,
                vCPU_to_Core_Ratio,
                Memory_Usage_Percent,
                Physical_Memory_MiB,
                Allocated_vMemory_MiB,
                VM_Count,
                VMs_per_Core,
                in_Maintenance_Mode,
                ESX_Version,
                ImportBatchId
            FROM [Reporting].[vw_Trends_Host_Utilization]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@HostName IS NULL OR HostName = @HostName)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
              AND SnapshotDate >= @StartDate
              AND SnapshotDate <= @EndDate
            ORDER BY HostName, SnapshotDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HostUtilizationItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.HostName,
            filter.Cluster,
            StartDate = filter.EffectiveStartDate,
            EndDate = filter.EffectiveEndDate
        });
    }

    /// <summary>
    /// Gets distinct host names for filter dropdown.
    /// </summary>
    public async Task<IEnumerable<string>> GetHostNamesAsync(string? viSdkServer = null, string? cluster = null)
    {
        const string sql = @"
            SELECT DISTINCT HostName
            FROM [Reporting].[vw_Trends_Host_Utilization]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
            ORDER BY HostName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { VI_SDK_Server = viSdkServer, Cluster = cluster });
    }

    /// <summary>
    /// Gets distinct cluster names for filter dropdown.
    /// </summary>
    public async Task<IEnumerable<string>> GetClusterNamesAsync(string? viSdkServer = null)
    {
        const string sql = @"
            SELECT DISTINCT Cluster
            FROM [Reporting].[vw_Trends_Host_Utilization]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND Cluster IS NOT NULL
            ORDER BY Cluster";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Builds chart data from the report items (line chart showing CPU and Memory utilization).
    /// </summary>
    public ChartDataViewModel BuildChartData(IEnumerable<HostUtilizationItem> items, string? hostName)
    {
        var chartData = new ChartDataViewModel();

        if (!items.Any())
            return chartData;

        if (!string.IsNullOrEmpty(hostName))
        {
            // Single host - show CPU and Memory utilization
            var filtered = items.Where(x => x.HostName == hostName).OrderBy(x => x.SnapshotDate).ToList();

            chartData.Labels = filtered.Select(d => d.SnapshotDate.ToString("MMM dd")).ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "CPU %",
                Data = filtered.Select(x => x.CPU_Usage_Percent).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.1)",
                Fill = false
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Memory %",
                Data = filtered.Select(x => x.Memory_Usage_Percent).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false
            });
        }
        else
        {
            // Multiple hosts - show average CPU and Memory per day
            var dates = items.Select(x => x.SnapshotDate).Distinct().OrderBy(x => x).ToList();
            chartData.Labels = dates.Select(d => d.ToString("MMM dd")).ToList();

            var aggregated = items
                .GroupBy(x => x.SnapshotDate)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgCPU = g.Where(x => x.CPU_Usage_Percent.HasValue).Any()
                        ? g.Where(x => x.CPU_Usage_Percent.HasValue).Average(x => x.CPU_Usage_Percent!.Value)
                        : (decimal?)null,
                    AvgMemory = g.Where(x => x.Memory_Usage_Percent.HasValue).Any()
                        ? g.Where(x => x.Memory_Usage_Percent.HasValue).Average(x => x.Memory_Usage_Percent!.Value)
                        : (decimal?)null,
                    MaxCPU = g.Where(x => x.CPU_Usage_Percent.HasValue).Any()
                        ? g.Where(x => x.CPU_Usage_Percent.HasValue).Max(x => x.CPU_Usage_Percent!.Value)
                        : (decimal?)null,
                    MaxMemory = g.Where(x => x.Memory_Usage_Percent.HasValue).Any()
                        ? g.Where(x => x.Memory_Usage_Percent.HasValue).Max(x => x.Memory_Usage_Percent!.Value)
                        : (decimal?)null
                })
                .ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Avg CPU %",
                Data = aggregated.Select(x => x.AvgCPU).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.1)",
                Fill = false,
                BorderWidth = 2
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Avg Memory %",
                Data = aggregated.Select(x => x.AvgMemory).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false,
                BorderWidth = 2
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Max CPU %",
                Data = aggregated.Select(x => x.MaxCPU).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.1)",
                Fill = false,
                BorderWidth = 1
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Max Memory %",
                Data = aggregated.Select(x => x.MaxMemory).ToList(),
                BorderColor = "#ffc107",
                BackgroundColor = "rgba(255, 193, 7, 0.1)",
                Fill = false,
                BorderWidth = 1
            });
        }

        return chartData;
    }
}
