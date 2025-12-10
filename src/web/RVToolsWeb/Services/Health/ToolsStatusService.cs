using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving VMware Tools Status report data.
/// </summary>
public class ToolsStatusService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ToolsStatusService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ToolsStatusItem>> GetReportDataAsync(ToolsStatusFilter filter)
    {
        const string sql = @"
            SELECT
                VM,
                VM_UUID,
                Powerstate,
                Template,
                ToolsStatus,
                Tools_Version,
                Required_Version,
                Upgradeable,
                Upgrade_Policy,
                App_status,
                Heartbeat_status,
                Operation_Ready,
                State_change_support,
                Interactive_Guest,
                Datacenter,
                Cluster,
                Host,
                Folder,
                OS_according_to_the_VMware_Tools,
                OS_according_to_the_configuration_file,
                VI_SDK_Server
            FROM [Reporting].[vw_Tools_Status]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY
                CASE ToolsStatus
                    WHEN 'toolsNotInstalled' THEN 1
                    WHEN 'toolsNotRunning' THEN 2
                    WHEN 'toolsOld' THEN 3
                    ELSE 4
                END,
                VM";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ToolsStatusItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
