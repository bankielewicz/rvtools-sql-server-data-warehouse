using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Shared;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving Datastore Capacity Trend report data.
/// </summary>
public class DatastoreCapacityTrendService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DatastoreCapacityTrendService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DatastoreCapacityTrendItem>> GetReportDataAsync(DatastoreCapacityTrendFilter filter)
    {
        const string sql = @"
            SELECT
                SnapshotDate,
                DatastoreName,
                VI_SDK_Server,
                Type,
                Capacity_MiB,
                Provisioned_MiB,
                In_Use_MiB,
                Free_MiB,
                Free_Percent,
                Num_VMs,
                ImportBatchId
            FROM [Reporting].[vw_Datastore_Capacity_Trend]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@DatastoreName IS NULL OR DatastoreName = @DatastoreName)
              AND SnapshotDate >= @StartDate
              AND SnapshotDate <= @EndDate
            ORDER BY DatastoreName, SnapshotDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DatastoreCapacityTrendItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.DatastoreName,
            StartDate = filter.EffectiveStartDate,
            EndDate = filter.EffectiveEndDate
        });
    }

    /// <summary>
    /// Gets distinct datastore names for filter dropdown.
    /// </summary>
    public async Task<IEnumerable<string>> GetDatastoreNamesAsync(string? viSdkServer = null)
    {
        const string sql = @"
            SELECT DISTINCT DatastoreName
            FROM [Reporting].[vw_Datastore_Capacity_Trend]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY DatastoreName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Builds chart data from the report items (line chart showing free percentage over time).
    /// </summary>
    public ChartDataViewModel BuildChartData(IEnumerable<DatastoreCapacityTrendItem> items, string? datastoreName)
    {
        var chartData = new ChartDataViewModel();

        if (!items.Any())
            return chartData;

        if (!string.IsNullOrEmpty(datastoreName))
        {
            // Single datastore - show free % and used %
            var filtered = items.Where(x => x.DatastoreName == datastoreName).OrderBy(x => x.SnapshotDate).ToList();

            chartData.Labels = filtered.Select(d => d.SnapshotDate.ToString("MMM dd")).ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Free %",
                Data = filtered.Select(x => x.Free_Percent).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false
            });

            // Calculate used %
            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Used %",
                Data = filtered.Select(x => x.Free_Percent.HasValue ? 100 - x.Free_Percent : null).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.1)",
                Fill = false
            });
        }
        else
        {
            // Multiple datastores - show average free % per day
            var dates = items.Select(x => x.SnapshotDate).Distinct().OrderBy(x => x).ToList();
            chartData.Labels = dates.Select(d => d.ToString("MMM dd")).ToList();

            var aggregated = items
                .Where(x => x.Free_Percent.HasValue)
                .GroupBy(x => x.SnapshotDate)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgFreePercent = g.Average(x => x.Free_Percent!.Value),
                    MinFreePercent = g.Min(x => x.Free_Percent!.Value),
                    MaxFreePercent = g.Max(x => x.Free_Percent!.Value)
                })
                .ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Avg Free %",
                Data = aggregated.Select(x => (decimal?)x.AvgFreePercent).ToList(),
                BorderColor = "#198754",
                BackgroundColor = "rgba(25, 135, 84, 0.1)",
                Fill = false,
                BorderWidth = 2
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Min Free %",
                Data = aggregated.Select(x => (decimal?)x.MinFreePercent).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.1)",
                Fill = false,
                BorderWidth = 1
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Max Free %",
                Data = aggregated.Select(x => (decimal?)x.MaxFreePercent).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.1)",
                Fill = false,
                BorderWidth = 1
            });
        }

        return chartData;
    }
}
