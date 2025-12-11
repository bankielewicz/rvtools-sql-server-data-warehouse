using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Shared;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving VM Count Trend report data.
/// </summary>
public class VMCountTrendService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMCountTrendService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VMCountTrendItem>> GetReportDataAsync(VMCountTrendFilter filter)
    {
        const string sql = @"
            SELECT
                SnapshotDate,
                VI_SDK_Server,
                VMCount,
                TemplateCount,
                PoweredOnCount,
                PoweredOffCount,
                SuspendedCount,
                ImportBatchId
            FROM [Reporting].[vw_VM_Count_Trend]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND SnapshotDate >= @StartDate
              AND SnapshotDate <= @EndDate
            ORDER BY SnapshotDate ASC, VI_SDK_Server";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMCountTrendItem>(sql, new
        {
            filter.VI_SDK_Server,
            StartDate = filter.EffectiveStartDate,
            EndDate = filter.EffectiveEndDate
        });
    }

    /// <summary>
    /// Builds chart data from the report items.
    /// </summary>
    public ChartDataViewModel BuildChartData(IEnumerable<VMCountTrendItem> items, string? viSdkServer)
    {
        var chartData = new ChartDataViewModel();

        if (!items.Any())
            return chartData;

        // Get unique dates for labels
        var dates = items.Select(x => x.SnapshotDate).Distinct().OrderBy(x => x).ToList();
        chartData.Labels = dates.Select(d => d.ToString("MMM dd")).ToList();

        if (string.IsNullOrEmpty(viSdkServer))
        {
            // Aggregate across all vCenters
            var aggregated = items
                .GroupBy(x => x.SnapshotDate)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.VMCount),
                    PoweredOn = g.Sum(x => x.PoweredOnCount),
                    PoweredOff = g.Sum(x => x.PoweredOffCount)
                })
                .ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Total VMs",
                Data = aggregated.Select(x => (decimal?)x.Total).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.1)",
                Fill = false
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Powered On",
                Data = aggregated.Select(x => (decimal?)x.PoweredOn).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Powered Off",
                Data = aggregated.Select(x => (decimal?)x.PoweredOff).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.1)",
                Fill = false
            });
        }
        else
        {
            // Single vCenter - show total, powered on, powered off
            var filtered = items.Where(x => x.VI_SDK_Server == viSdkServer).OrderBy(x => x.SnapshotDate).ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Total VMs",
                Data = filtered.Select(x => (decimal?)x.VMCount).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.1)",
                Fill = false
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Powered On",
                Data = filtered.Select(x => (decimal?)x.PoweredOnCount).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Powered Off",
                Data = filtered.Select(x => (decimal?)x.PoweredOffCount).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.1)",
                Fill = false
            });
        }

        return chartData;
    }
}
