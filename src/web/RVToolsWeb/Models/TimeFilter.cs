namespace RVToolsWeb.Models;

/// <summary>
/// Global time filter options for data display
/// </summary>
public static class TimeFilter
{
    public const string Latest = "latest";
    public const string Last7Days = "7d";
    public const string Last30Days = "30d";
    public const string Last90Days = "90d";
    public const string LastYear = "1y";
    public const string AllTime = "all";

    public static readonly string DefaultFilter = Last30Days;

    public static readonly Dictionary<string, string> Options = new()
    {
        { Latest, "Latest Import" },
        { Last7Days, "Last 7 Days" },
        { Last30Days, "Last 30 Days" },
        { Last90Days, "Last 90 Days" },
        { LastYear, "Last Year" },
        { AllTime, "All Time" }
    };

    public static bool IsValid(string? filter) =>
        filter != null && Options.ContainsKey(filter);

    /// <summary>
    /// Convert filter to SQL WHERE clause fragment
    /// </summary>
    public static string ToSqlClause(string filter, string dateColumn = "LastModifiedDate")
    {
        return filter switch
        {
            Latest => $"AND {dateColumn} >= (SELECT MAX(ImportStartTime) FROM Audit.ImportBatch WHERE Status = 'Completed')",
            Last7Days => $"AND {dateColumn} >= DATEADD(DAY, -7, GETDATE())",
            Last30Days => $"AND {dateColumn} >= DATEADD(DAY, -30, GETDATE())",
            Last90Days => $"AND {dateColumn} >= DATEADD(DAY, -90, GETDATE())",
            LastYear => $"AND {dateColumn} >= DATEADD(YEAR, -1, GETDATE())",
            AllTime => "",
            _ => $"AND {dateColumn} >= DATEADD(DAY, -30, GETDATE())"
        };
    }
}
