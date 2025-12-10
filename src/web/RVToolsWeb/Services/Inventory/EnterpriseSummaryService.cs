using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Enterprise Summary report data.
/// </summary>
public class EnterpriseSummaryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public EnterpriseSummaryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<EnterpriseSummaryItem>> GetReportDataAsync(EnterpriseSummaryFilter filter)
    {
        const string sql = @"
            SELECT
                VI_SDK_Server,
                VMs_PoweredOn,
                VMs_PoweredOff,
                Templates,
                Total_VMs,
                Total_vCPUs,
                Total_vMemory_MiB,
                Total_Provisioned_MiB,
                Total_InUse_MiB,
                Cluster_Count,
                Host_Count,
                Datacenter_Count,
                Latest_ImportBatchId,
                Latest_Import_Date
            FROM [Reporting].[vw_MultiVCenter_Enterprise_Summary]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY VI_SDK_Server";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<EnterpriseSummaryItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
