using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving Snapshot Aging report data.
/// </summary>
public class SnapshotAgingService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SnapshotAgingService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SnapshotAgingItem>> GetReportDataAsync(SnapshotAgingFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                Powerstate,
                SnapshotName,
                Description,
                SnapshotDate,
                Filename,
                Size_MiB_vmsn,
                Size_MiB_total,
                AgeDays,
                Quiesced,
                State,
                Datacenter,
                Cluster,
                Host,
                Folder,
                OS_according_to_the_VMware_Tools,
                VI_SDK_Server
            FROM [Reporting].[vw_Snapshot_Aging]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND AgeDays >= @MinAgeDays
            ORDER BY AgeDays DESC, Size_MiB_total DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SnapshotAgingItem>(sql, new
        {
            filter.VI_SDK_Server,
            filter.MinAgeDays
        });
    }
}
