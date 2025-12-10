namespace RVToolsWeb.Configuration;

/// <summary>
/// Strongly-typed configuration for appsettings.json
/// </summary>
public class AppSettings
{
    public ConnectionStringsConfig ConnectionStrings { get; set; } = new();
    public UiConfig Ui { get; set; } = new();
    public CachingConfig Caching { get; set; } = new();
    public WebLoggingConfig WebLogging { get; set; } = new();
}

/// <summary>
/// Database connection strings
/// </summary>
public class ConnectionStringsConfig
{
    public string RVToolsDW { get; set; } = string.Empty;
}

/// <summary>
/// UI customization settings
/// </summary>
public class UiConfig
{
    public string LogoPath { get; set; } = "/images/logo-placeholder.png";
    public string ApplicationTitle { get; set; } = "RVTools Data Warehouse";
}

/// <summary>
/// Caching configuration
/// </summary>
public class CachingConfig
{
    public int FilterCacheMinutes { get; set; } = 5;
}

/// <summary>
/// Web logging configuration
/// </summary>
public class WebLoggingConfig
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Warning";
    public DatabaseLoggingConfig DatabaseLogging { get; set; } = new();
    public ConsoleLoggingConfig ConsoleLogging { get; set; } = new();
    public bool IncludeRequestHeaders { get; set; } = true;
    public List<string> ExcludedHeaders { get; set; } = new() { "Authorization", "Cookie", "X-CSRF-Token" };
    public int MaxStackTraceLength { get; set; } = 8000;
    public int RetentionDays { get; set; } = 30;
}

/// <summary>
/// Database logging sink configuration
/// </summary>
public class DatabaseLoggingConfig
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Error";
}

/// <summary>
/// Console logging sink configuration
/// </summary>
public class ConsoleLoggingConfig
{
    public bool Enabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
}
