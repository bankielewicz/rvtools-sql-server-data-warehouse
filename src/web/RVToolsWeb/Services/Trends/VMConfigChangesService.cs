using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Trends;
using Dapper;

namespace RVToolsWeb.Services.Trends;

/// <summary>
/// Service for retrieving VM Configuration Changes report data.
/// </summary>
public class VMConfigChangesService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public VMConfigChangesService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<VMConfigChangesItem>> GetReportDataAsync(VMConfigChangesFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                VI_SDK_Server,
                EffectiveFrom,
                EffectiveUntil,
                ChangedDate,
                Powerstate,
                CPUs,
                Memory_MB,
                NICs,
                Disks,
                Datacenter,
                Cluster,
                Host,
                HW_version,
                OS_according_to_the_VMware_Tools,
                SourceFile,
                ImportBatchId
            FROM [Reporting].[vw_VM_Config_Changes]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@VMName IS NULL OR VM LIKE '%' + @VMName + '%')
              AND ChangedDate >= @StartDate
              AND ChangedDate <= @EndDate
            ORDER BY ChangedDate DESC, VM";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<VMConfigChangesItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.VMName,
            StartDate = filter.EffectiveStartDate,
            EndDate = filter.EffectiveEndDate
        });
    }
}
