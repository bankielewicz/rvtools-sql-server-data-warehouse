using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving VM Lifecycle report data.
/// </summary>
public class VMLifecycleService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMLifecycleService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VMLifecycleItem>> GetReportDataAsync(VMLifecycleFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                Host,
                Resource_pool,
                Powerstate,
                State_Start_Date,
                State_End_Date,
                Days_In_State,
                Last_PowerOn_Time,
                Template,
                OS_according_to_the_VMware_Tools,
                ImportBatchId,
                ValidFrom,
                ValidTo
            FROM [Reporting].[vw_Trends_VM_Lifecycle]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@VMName IS NULL OR VM LIKE '%' + @VMName + '%')
              AND (@Powerstate IS NULL OR @Powerstate = '' OR Powerstate = @Powerstate)
              AND State_Start_Date >= @StartDate
              AND State_Start_Date <= @EndDate
            ORDER BY VM, State_Start_Date DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMLifecycleItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.VMName,
            filter.Powerstate,
            StartDate = filter.EffectiveStartDate,
            EndDate = filter.EffectiveEndDate
        });
    }
}
