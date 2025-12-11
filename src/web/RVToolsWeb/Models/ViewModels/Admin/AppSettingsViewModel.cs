namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Editable application settings from appsettings.json.
/// </summary>
public class AppSettingsViewModel
{
    // UI Section
    public string ApplicationTitle { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;

    // Caching Section
    public int FilterCacheMinutes { get; set; }

    // WebLogging Section
    public bool LoggingEnabled { get; set; }
    public string LoggingMinimumLevel { get; set; } = string.Empty;
    public bool DatabaseLoggingEnabled { get; set; }
    public string DatabaseLoggingMinimumLevel { get; set; } = string.Empty;
    public bool ConsoleLoggingEnabled { get; set; }
    public string ConsoleLoggingMinimumLevel { get; set; } = string.Empty;
    public int LogRetentionDays { get; set; }

    // Read-only display (connection string - masked)
    public string ConnectionStringMasked { get; set; } = string.Empty;

    /// <summary>
    /// Available log levels for dropdowns.
    /// </summary>
    public static IEnumerable<string> LogLevels => new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" };
}
