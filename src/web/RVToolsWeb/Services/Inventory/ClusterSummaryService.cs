using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving Cluster Summary report data.
/// </summary>
public class ClusterSummaryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClusterSummaryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClusterSummaryItem>> GetReportDataAsync(ClusterSummaryFilter filter)
    {
        const string sql = @"
            SELECT
                ClusterName,
                VI_SDK_Server,
                Config_status,
                OverallStatus,
                NumHosts,
                numEffectiveHosts,
                TotalCpu,
                NumCpuCores,
                NumCpuThreads,
                Effective_Cpu,
                TotalMemory,
                Effective_Memory,
                HA_enabled,
                Failover_Level,
                AdmissionControlEnabled,
                Host_monitoring,
                Isolation_Response,
                Restart_Priority,
                VM_Monitoring,
                Max_Failures,
                Max_Failure_Window,
                Failure_Interval,
                Min_Up_Time,
                DRS_enabled,
                DRS_default_VM_behavior,
                DRS_vmotion_rate,
                DPM_enabled,
                DPM_default_behavior,
                Num_VMotions
            FROM [Reporting].[vw_Cluster_Summary]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY VI_SDK_Server, ClusterName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ClusterSummaryItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
