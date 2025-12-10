using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Host Inventory report data.
/// </summary>
public class HostInventoryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public HostInventoryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HostInventoryItem>> GetReportDataAsync(HostInventoryFilter filter)
    {
        const string sql = @"
            SELECT
                Host,
                UUID,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                Config_status,
                in_Maintenance_Mode,
                in_Quarantine_Mode,
                CPU_Model,
                Speed,
                Num_CPU,
                Cores_per_CPU,
                Num_Cores,
                HT_Available,
                HT_Active,
                Num_Memory,
                Num_NICs,
                Num_HBAs,
                Num_VMs,
                Num_vCPUs,
                vCPUs_per_Core,
                vRAM,
                ESX_Version,
                Current_EVC,
                Max_EVC,
                Vendor,
                Model,
                Serial_number,
                Service_tag,
                BIOS_Version,
                BIOS_Date,
                Boot_time,
                Time_Zone_Name,
                Certificate_Expiry_Date,
                Certificate_Status
            FROM [Reporting].[vw_Host_Inventory]
            WHERE (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
              AND (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY Datacenter, Cluster, Host";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HostInventoryItem>(sql, new
        {
            filter.Datacenter,
            filter.Cluster,
            filter.VI_SDK_Server
        });
    }
}
