using RVToolsWeb.Data;
using RVToolsWeb.Models.ViewModels.Health;
using Dapper;

namespace RVToolsWeb.Services.Health;

/// <summary>
/// Service for retrieving Health Issues report data.
/// </summary>
public class HealthIssuesService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public HealthIssuesService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<HealthIssuesItem>> GetReportDataAsync(HealthIssuesFilter filter)
    {
        const string sql = @"
            SELECT
                ObjectName,
                Message,
                IssueType,
                VI_SDK_Server,
                ImportBatchId,
                DetectedDate
            FROM [Reporting].[vw_Health_Issues]
            WHERE (@VI_SDK_Server IS NULL OR VI_SDK_Server = @VI_SDK_Server)
            ORDER BY IssueType, ObjectName";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<HealthIssuesItem>(sql, new
        {
            filter.VI_SDK_Server
        });
    }
}
