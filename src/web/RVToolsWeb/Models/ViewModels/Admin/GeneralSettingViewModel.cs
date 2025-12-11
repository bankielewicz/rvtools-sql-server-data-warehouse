namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Represents a single row from Config.Settings.
/// </summary>
public class GeneralSettingViewModel
{
    public int SettingId { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Display-friendly name with underscores replaced by spaces.
    /// </summary>
    public string DisplayName => SettingName.Replace("_", " ");
}
