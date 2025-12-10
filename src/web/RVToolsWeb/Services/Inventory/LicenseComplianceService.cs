using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Inventory;
using Dapper;

namespace RVToolsWeb.Services.Inventory;

/// <summary>
/// Service for retrieving License Compliance report data.
/// </summary>
public class LicenseComplianceService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public LicenseComplianceService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LicenseComplianceItem>> GetReportDataAsync(LicenseComplianceFilter filter)
    {
        const string sql = @"
            SELECT
                LicenseName,
                LicenseKey,
                Labels,
                Cost_Unit,
                Total_Licenses,
                Used_Licenses,
                Available_Licenses,
                Usage_Percent,
                Expiration_Date,
                Days_Until_Expiration,
                Compliance_Status,
                Features,
                VI_SDK_Server
            FROM [Reporting].[vw_Inventory_License_Compliance]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY
                CASE Compliance_Status
                    WHEN 'Over-Allocated' THEN 1
                    WHEN 'Expired' THEN 2
                    WHEN 'Expiring Soon' THEN 3
                    WHEN 'Near Capacity' THEN 4
                    ELSE 5
                END,
                Days_Until_Expiration,
                LicenseName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<LicenseComplianceItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
