using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Capacity;
using Dapper;

namespace RVToolsWeb.Services.Capacity;

/// <summary>
/// Service for retrieving VM Right-Sizing report data.
/// </summary>
public class VMRightSizingService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMRightSizingService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VMRightSizingItem>> GetReportDataAsync(VMRightSizingFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                VI_SDK_Server,
                Powerstate,
                Datacenter,
                Cluster,
                Host,
                Resource_pool,
                CPU_Allocated,
                CPU_Readiness_Percent,
                CPU_Reservation_MHz,
                Memory_Allocated_MiB,
                Memory_Active_MiB,
                Memory_Consumed_MiB,
                Memory_Ballooned_MiB,
                Memory_Reservation_MiB,
                Memory_Active_Percent,
                Memory_Reservation_Percent,
                OS_according_to_the_VMware_Tools,
                ImportBatchId,
                LastModifiedDate
            FROM [Reporting].[vw_Capacity_VM_RightSizing]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
              AND (@MaxActivePercent IS NULL OR Memory_Active_Percent < @MaxActivePercent OR Memory_Active_Percent IS NULL)
            ORDER BY
                Memory_Active_Percent ASC,
                Memory_Allocated_MiB DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMRightSizingItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.Datacenter,
            filter.Cluster,
            filter.MaxActivePercent
        });
    }
}
