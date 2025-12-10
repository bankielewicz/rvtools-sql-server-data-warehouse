using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving Configuration Compliance report data.
/// </summary>
public class ConfigurationComplianceService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ConfigurationComplianceService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ConfigurationComplianceItem>> GetReportDataAsync(ConfigurationComplianceFilter filter)
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
                OS_according_to_the_VMware_Tools,
                CPU_Count,
                Host_Physical_Cores,
                vCPU_to_Core_Ratio,
                Memory_Allocated_MiB,
                Memory_Reservation_MiB,
                Memory_Reservation_Percent,
                Boot_Delay_Seconds,
                Tools_Status,
                Tools_Version,
                Tools_Upgradeable,
                vCPU_Ratio_Compliant,
                Memory_Reservation_Compliant,
                Boot_Delay_Compliant,
                Tools_Compliant,
                Overall_Compliance_Status
            FROM [Reporting].[vw_Health_Configuration_Compliance]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@ShowNonCompliantOnly = 0 OR Overall_Compliance_Status = 'Non-Compliant')
            ORDER BY Overall_Compliance_Status DESC, VM";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<ConfigurationComplianceItem>(sql, new
        {
            filter.VI_SDK_Server,
            ShowNonCompliantOnly = filter.ShowNonCompliantOnly ? 1 : 0
        });
    }
}
