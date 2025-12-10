using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Capacity;
using Dapper;

namespace RVToolsWeb.Services.Capacity;

/// <summary>
/// Service for retrieving Datastore Capacity report data.
/// </summary>
public class DatastoreCapacityService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DatastoreCapacityService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DatastoreCapacityItem>> GetReportDataAsync(DatastoreCapacityFilter filter)
    {
        const string sql = @"
            SELECT
                DatastoreName,
                VI_SDK_Server,
                Type,
                Cluster_name,
                Capacity_MiB,
                Provisioned_MiB,
                In_Use_MiB,
                Free_MiB,
                Free_Percent,
                OverProvisioningPercent,
                CapacityStatus,
                Num_VMs,
                Num_Hosts,
                Accessible,
                ImportBatchId,
                LastModifiedDate
            FROM [Reporting].[vw_Datastore_Capacity]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@StatusFilter IS NULL OR @StatusFilter = 'All' OR CapacityStatus = @StatusFilter)
            ORDER BY
                CASE CapacityStatus
                    WHEN 'Critical' THEN 1
                    WHEN 'Warning' THEN 2
                    ELSE 3
                END,
                Free_Percent ASC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DatastoreCapacityItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.StatusFilter
        });
    }
}
