using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Capacity;
using Dapper;

namespace RVToolsWeb.Services.Capacity;

/// <summary>
/// Service for retrieving VM Resource Allocation report data.
/// </summary>
public class VMResourceAllocationService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMResourceAllocationService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VMResourceAllocationItem>> GetReportDataAsync(VMResourceAllocationFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                VI_SDK_Server,
                Powerstate,
                Template,
                Datacenter,
                Cluster,
                Host,
                CPU_Count,
                CPU_Sockets,
                Cores_Per_Socket,
                CPU_Overall_MHz,
                CPU_Reservation_MHz,
                CPU_Limit_MHz,
                CPU_Shares_Level,
                CPU_Hot_Add,
                Memory_Size_MiB,
                Memory_Consumed_MiB,
                Memory_Active_MiB,
                Memory_Ballooned_MiB,
                Memory_Swapped_MiB,
                Memory_Reservation_MiB,
                Memory_Limit_MiB,
                Memory_Shares_Level,
                Memory_Hot_Add,
                OS_according_to_the_VMware_Tools,
                ImportBatchId,
                LastModifiedDate
            FROM [Reporting].[vw_VM_Resource_Allocation]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
              AND (@Powerstate IS NULL OR @Powerstate = '' OR Powerstate = @Powerstate)
            ORDER BY
                Memory_Size_MiB DESC,
                CPU_Count DESC,
                VM";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMResourceAllocationItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.Datacenter,
            filter.Cluster,
            filter.Powerstate
        });
    }
}
