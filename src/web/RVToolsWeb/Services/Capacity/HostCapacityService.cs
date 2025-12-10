using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Capacity;
using Dapper;

namespace RVToolsWeb.Services.Capacity;

/// <summary>
/// Service for retrieving Host Capacity report data.
/// </summary>
public class HostCapacityService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public HostCapacityService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HostCapacityItem>> GetReportDataAsync(HostCapacityFilter filter)
    {
        const string sql = @"
            SELECT
                Host,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                Num_Cores,
                CPU_Usage_Percent,
                CPUStatus,
                Memory_MB,
                Memory_Usage_Percent,
                MemoryStatus,
                Num_VMs,
                Num_vCPUs,
                vCPUs_per_Core,
                vRAM_MB,
                in_Maintenance_Mode,
                ImportBatchId,
                LastModifiedDate
            FROM [Reporting].[vw_Host_Capacity]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@Datacenter IS NULL OR Datacenter = @Datacenter)
              AND (@Cluster IS NULL OR Cluster = @Cluster)
            ORDER BY
                CASE
                    WHEN CPUStatus = 'Critical' OR MemoryStatus = 'Critical' THEN 1
                    WHEN CPUStatus = 'Warning' OR MemoryStatus = 'Warning' THEN 2
                    ELSE 3
                END,
                Datacenter, Cluster, Host";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HostCapacityItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.Datacenter,
            filter.Cluster
        });
    }
}
