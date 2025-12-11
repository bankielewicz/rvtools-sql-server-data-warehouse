namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Represents a table retention override from Config.TableRetention.
/// </summary>
public class TableRetentionViewModel
{
    public int TableRetentionId { get; set; }
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Full qualified table name (Schema.Table).
    /// </summary>
    public string FullTableName => $"{SchemaName}.{TableName}";
}
