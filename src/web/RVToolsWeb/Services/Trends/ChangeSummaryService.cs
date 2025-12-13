using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models;
using RVToolsWeb.Models.ViewModels.Trends;

namespace RVToolsWeb.Services.Trends;

public interface IChangeSummaryService
{
    Task<ChangeSummaryViewModel> GetChangeSummaryAsync(ChangeSummaryFilter filter);
    Task<DashboardChangeWidget> GetDashboardWidgetDataAsync(string timeFilter);
}

public class ChangeSummaryService : IChangeSummaryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ChangeSummaryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ChangeSummaryViewModel> GetChangeSummaryAsync(ChangeSummaryFilter filter)
    {
        var dateClause = GetDateClause(filter.TimeFilter ?? TimeFilter.Last30Days);
        var vcenterClause = string.IsNullOrEmpty(filter.VI_SDK_Server)
            ? "" : "AND VI_SDK_Server = @VI_SDK_Server";

        var sql = $@"
            SELECT VM, VM_UUID, Powerstate, Host, Cluster, Datacenter,
                   VI_SDK_Server, ChangeDate, ChangeType, CPUs, Memory, Provisioned_MiB
            FROM [Reporting].[vw_Change_Summary]
            WHERE 1=1 {dateClause} {vcenterClause}
            ORDER BY ChangeDate DESC";

        using var connection = _connectionFactory.CreateConnection();
        var items = await connection.QueryAsync<ChangeSummaryItem>(sql, filter);

        return new ChangeSummaryViewModel
        {
            Filter = filter,
            Items = items,
            CreatedCount = items.Count(i => i.ChangeType == "Created"),
            DeletedCount = items.Count(i => i.ChangeType == "Deleted")
        };
    }

    public async Task<DashboardChangeWidget> GetDashboardWidgetDataAsync(string timeFilter)
    {
        var dateClause = GetDateClause(timeFilter);

        var sql = $@"
            SELECT
                SUM(CASE WHEN ChangeType = 'Created' THEN 1 ELSE 0 END) AS VMsCreated,
                SUM(CASE WHEN ChangeType = 'Deleted' THEN 1 ELSE 0 END) AS VMsDeleted,
                COUNT(DISTINCT VI_SDK_Server) AS VCentersAffected
            FROM [Reporting].[vw_Change_Summary]
            WHERE 1=1 {dateClause}";

        using var connection = _connectionFactory.CreateConnection();
        var summary = await connection.QueryFirstOrDefaultAsync<DashboardChangeWidget>(sql);

        // Get weekly breakdown for chart
        var weeklySql = $@"
            SELECT
                DATEPART(WEEK, ChangeDate) AS WeekNumber,
                MIN(ChangeDate) AS WeekStart,
                SUM(CASE WHEN ChangeType = 'Created' THEN 1 ELSE 0 END) AS Created,
                SUM(CASE WHEN ChangeType = 'Deleted' THEN 1 ELSE 0 END) AS Deleted
            FROM [Reporting].[vw_Change_Summary]
            WHERE 1=1 {dateClause}
            GROUP BY DATEPART(WEEK, ChangeDate)
            ORDER BY WeekNumber";

        var weeklyData = await connection.QueryAsync<WeeklyChangeData>(weeklySql);
        summary ??= new DashboardChangeWidget();
        summary.WeeklyData = weeklyData.ToList();
        summary.NetChange = summary.VMsCreated - summary.VMsDeleted;

        return summary;
    }

    private static string GetDateClause(string timeFilter)
    {
        return timeFilter switch
        {
            TimeFilter.Latest => "AND ChangeDate >= CAST(GETDATE() AS DATE)",
            TimeFilter.Last7Days => "AND ChangeDate >= DATEADD(DAY, -7, GETDATE())",
            TimeFilter.Last30Days => "AND ChangeDate >= DATEADD(DAY, -30, GETDATE())",
            TimeFilter.Last90Days => "AND ChangeDate >= DATEADD(DAY, -90, GETDATE())",
            TimeFilter.LastYear => "AND ChangeDate >= DATEADD(YEAR, -1, GETDATE())",
            TimeFilter.AllTime => "",
            _ => "AND ChangeDate >= DATEADD(DAY, -30, GETDATE())"
        };
    }
}
