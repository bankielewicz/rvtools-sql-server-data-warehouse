namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Aggregated view model for the Settings page with all tab data.
/// </summary>
public class SettingsIndexViewModel
{
    public string ActiveTab { get; set; } = "general";

    // Tab 1: General Settings
    public IEnumerable<GeneralSettingViewModel> GeneralSettings { get; set; } = Enumerable.Empty<GeneralSettingViewModel>();

    // Tab 2: Table Retention
    public IEnumerable<TableRetentionViewModel> TableRetentions { get; set; } = Enumerable.Empty<TableRetentionViewModel>();
    public IEnumerable<string> AvailableTables { get; set; } = Enumerable.Empty<string>();

    // Tab 3: Application Settings
    public AppSettingsViewModel AppSettings { get; set; } = new();

    // Tab 4: Database Status
    public DatabaseStatusViewModel DatabaseStatus { get; set; } = new();

    // Tab 5: Metadata (read-only)
    public IEnumerable<TableMappingViewModel> TableMappings { get; set; } = Enumerable.Empty<TableMappingViewModel>();
    public int TotalColumnMappings { get; set; }

    // Tab 6: Environment
    public EnvironmentViewModel Environment { get; set; } = new();

    // Tab 7: Security
    public SecurityViewModel Security { get; set; } = new();
}
