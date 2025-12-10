using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Shared;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving Storage Growth report data.
/// </summary>
public class StorageGrowthService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public StorageGrowthService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<StorageGrowthItem>> GetReportDataAsync(StorageGrowthFilter filter)
    {
        const string sql = @"
            SELECT
                SnapshotDate,
                DatastoreName,
                VI_SDK_Server,
                Type,
                Capacity_MiB,
                In_Use_MiB,
                Free_MiB,
                Free_Percent,
                DayNumber,
                ImportBatchId
            FROM [Reporting].[vw_Trends_Storage_Growth]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@DatastoreName IS NULL OR DatastoreName = @DatastoreName)
              AND SnapshotDate >= DATEADD(DAY, -@LookbackDays, CAST(GETUTCDATE() AS DATE))
            ORDER BY DatastoreName, SnapshotDate ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<StorageGrowthItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.DatastoreName,
            filter.LookbackDays
        });
    }

    /// <summary>
    /// Gets distinct datastore names for filter dropdown.
    /// </summary>
    public async Task<IEnumerable<string>> GetDatastoreNamesAsync(string? viSdkServer = null)
    {
        const string sql = @"
            SELECT DISTINCT DatastoreName
            FROM [Reporting].[vw_Trends_Storage_Growth]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY DatastoreName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<string>(sql, new { VI_SDK_Server = viSdkServer });
    }

    /// <summary>
    /// Builds chart data from the report items (area chart showing used storage).
    /// </summary>
    public ChartDataViewModel BuildChartData(IEnumerable<StorageGrowthItem> items, string? datastoreName)
    {
        var chartData = new ChartDataViewModel();

        if (!items.Any())
            return chartData;

        if (!string.IsNullOrEmpty(datastoreName))
        {
            // Single datastore - show used and capacity
            var filtered = items.Where(x => x.DatastoreName == datastoreName).OrderBy(x => x.SnapshotDate).ToList();

            chartData.Labels = filtered.Select(d => d.SnapshotDate.ToString("MMM dd")).ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Used (GiB)",
                Data = filtered.Select(x => (decimal?)(x.In_Use_MiB / 1024.0m)).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.2)",
                Fill = true
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Capacity (GiB)",
                Data = filtered.Select(x => (decimal?)(x.Capacity_MiB / 1024.0m)).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.0)",
                Fill = false,
                BorderWidth = 1
            });
        }
        else
        {
            // Aggregate all datastores - show total used per day
            var dates = items.Select(x => x.SnapshotDate).Distinct().OrderBy(x => x).ToList();
            chartData.Labels = dates.Select(d => d.ToString("MMM dd")).ToList();

            var aggregated = items
                .GroupBy(x => x.SnapshotDate)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalUsed = g.Sum(x => x.In_Use_MiB) / 1024.0m,
                    TotalCapacity = g.Sum(x => x.Capacity_MiB) / 1024.0m
                })
                .ToList();

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Total Used (GiB)",
                Data = aggregated.Select(x => (decimal?)x.TotalUsed).ToList(),
                BorderColor = "#dc3545",
                BackgroundColor = "rgba(220, 53, 69, 0.2)",
                Fill = true
            });

            chartData.Datasets.Add(new ChartDataset
            {
                Label = "Total Capacity (GiB)",
                Data = aggregated.Select(x => (decimal?)x.TotalCapacity).ToList(),
                BorderColor = "#0d6efd",
                BackgroundColor = "rgba(13, 110, 253, 0.0)",
                Fill = false,
                BorderWidth = 1
            });
        }

        return chartData;
    }
}
