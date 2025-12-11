namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Read-only view of Config.TableMapping.
/// </summary>
public class TableMappingViewModel
{
    public string TableName { get; set; } = string.Empty;
    public string NaturalKeyColumns { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ColumnCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
