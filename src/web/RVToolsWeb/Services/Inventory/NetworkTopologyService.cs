using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Network Topology report data.
/// </summary>
public class NetworkTopologyService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public NetworkTopologyService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<NetworkTopologyItem>> GetReportDataAsync(NetworkTopologyFilter filter)
    {
        const string sql = @"
            SELECT
                Port_Group,
                VLAN,
                Switch_Name,
                HostName,
                Datacenter,
                Cluster,
                VI_SDK_Server,
                Switch_Type,
                VM_Count,
                Is_Orphaned,
                Promiscuous_Mode,
                Mac_Changes,
                Forged_Transmits,
                Traffic_Shaping,
                Load_Balancing_Policy
            FROM [Reporting].[vw_Inventory_Network_Topology]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@ShowOrphanedOnly = 0 OR Is_Orphaned = 1)
            ORDER BY VI_SDK_Server, VLAN, Port_Group";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<NetworkTopologyItem>(sql, new
        {
            filter.VI_SDK_Server,
            ShowOrphanedOnly = filter.ShowOrphanedOnly ? 1 : 0
        });
    }
}
