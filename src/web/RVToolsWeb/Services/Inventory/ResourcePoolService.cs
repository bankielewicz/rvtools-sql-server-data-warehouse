using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Resource Pool Utilization report data.
/// </summary>
public class ResourcePoolService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ResourcePoolService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ResourcePoolItem>> GetReportDataAsync(ResourcePoolFilter filter)
    {
        const string sql = @"
            SELECT
                ResourcePool,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                VMs_PoweredOn,
                Total_VMs,
                Total_vCPUs,
                Total_CPU_Reservation_MHz,
                Total_CPU_Limit_MHz,
                Total_Memory_MiB,
                Total_Active_Memory_MiB,
                Total_Memory_Reservation_MiB,
                Total_Memory_Limit_MiB,
                Avg_Memory_Active_Percent
            FROM [Reporting].[vw_MultiVCenter_ResourcePool_Utilization]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY VI_SDK_Server, Datacenter, Cluster, ResourcePool";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ResourcePoolItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
