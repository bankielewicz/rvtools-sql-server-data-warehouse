using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving Certificate Expiration report data.
/// </summary>
public class CertificateExpirationService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CertificateExpirationService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<CertificateExpirationItem>> GetReportDataAsync(CertificateExpirationFilter filter)
    {
        const string sql = @"
            SELECT
                HostName,
                VI_SDK_Server,
                Datacenter,
                Cluster,
                Certificate_Issuer,
                Certificate_Subject,
                Certificate_Status,
                Certificate_Start_Date,
                Certificate_Expiry_Date,
                Days_Until_Expiration,
                Expiration_Status,
                ESX_Version
            FROM [Reporting].[vw_Health_Certificate_Expiration]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
              AND (@IncludeValid = 1 OR Expiration_Status <> 'Valid')
            ORDER BY Days_Until_Expiration, HostName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<CertificateExpirationItem>(sql, new
        {
            filter.VI_SDK_Server,
            IncludeValid = filter.IncludeValid ? 1 : 0
        });
    }
}
