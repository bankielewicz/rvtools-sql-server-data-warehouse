using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving VM Inventory report data.
/// </summary>
public class VMInventoryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMInventoryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Gets VM inventory data with optional filtering.
    /// </summary>
    public async Task<IEnumerable<VMInventoryItem>> GetReportDataAsync(VMInventoryFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                VI_SDK_Server,
                Powerstate,
                Template,
                Config_status,
                Guest_state,
                CPUs,
                Memory,
                NICs,
                Disks,
                Total_disk_capacity_MiB,
                Provisioned_MiB,
                In_Use_MiB,
                Primary_IP_Address,
                DNS_Name,
                Datacenter,
                Cluster,
                Host,
                Folder,
                Resource_pool,
                OS_according_to_the_VMware_Tools,
                OS_according_to_the_configuration_file,
                HW_version,
                Firmware,
                Creation_date,
                PowerOn,
                Annotation,
                Path
            FROM [Reporting].[vw_VM_Inventory]
            WHERE (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
              AND (@Powerstate IS NULL OR Powerstate = @Powerstate)
              AND (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY Datacenter, Cluster, VM";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMInventoryItem>(sql, new
        {
            filter.Datacenter,
            filter.Cluster,
            filter.Powerstate,
            filter.VI_SDK_Server
        });
    }
}
