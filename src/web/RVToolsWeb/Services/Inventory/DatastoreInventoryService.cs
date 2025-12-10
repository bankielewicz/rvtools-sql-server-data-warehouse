using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Datastore Inventory report data.
/// </summary>
public class DatastoreInventoryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DatastoreInventoryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DatastoreInventoryItem>> GetReportDataAsync(DatastoreInventoryFilter filter)
    {
        const string sql = @"
            SELECT
                DatastoreName,
                VI_SDK_Server,
                Config_status,
                Accessible,
                Type,
                Major_Version,
                Version,
                VMFS_Upgradeable,
                Capacity_MiB,
                Provisioned_MiB,
                In_Use_MiB,
                Free_MiB,
                Free_Percent,
                Num_VMs,
                Num_Hosts,
                SIOC_enabled,
                SIOC_Threshold,
                Cluster_name,
                Cluster_capacity_MiB,
                Cluster_free_space_MiB,
                Block_size,
                URL
            FROM [Reporting].[vw_Datastore_Inventory]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY VI_SDK_Server, DatastoreName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<DatastoreInventoryItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
