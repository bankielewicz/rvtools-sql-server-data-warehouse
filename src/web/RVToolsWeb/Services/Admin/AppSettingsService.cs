using System.Text.Json;
using System.Text.Json.Nodes;
using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service for managing appsettings.json configuration.
/// </summary>
public class AppSettingsService : IAppSettingsService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppSettingsService> _logger;

    public AppSettingsService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<AppSettingsService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<AppSettingsViewModel> GetAppSettingsAsync()
    {
        var settings = new AppSettingsViewModel
        {
            // UI Section
            ApplicationTitle = _configuration["Ui:ApplicationTitle"] ?? "RVTools Data Warehouse",
            LogoPath = _configuration["Ui:LogoPath"] ?? "/images/logo-placeholder.png",

            // Caching Section
            FilterCacheMinutes = _configuration.GetValue<int>("Caching:FilterCacheMinutes", 5),

            // WebLogging Section
            LoggingEnabled = _configuration.GetValue<bool>("WebLogging:Enabled", true),
            LoggingMinimumLevel = _configuration["WebLogging:MinimumLevel"] ?? "Warning",
            DatabaseLoggingEnabled = _configuration.GetValue<bool>("WebLogging:DatabaseLogging:Enabled", true),
            DatabaseLoggingMinimumLevel = _configuration["WebLogging:DatabaseLogging:MinimumLevel"] ?? "Error",
            ConsoleLoggingEnabled = _configuration.GetValue<bool>("WebLogging:ConsoleLogging:Enabled", true),
            ConsoleLoggingMinimumLevel = _configuration["WebLogging:ConsoleLogging:MinimumLevel"] ?? "Information",
            LogRetentionDays = _configuration.GetValue<int>("WebLogging:RetentionDays", 30),

            // Connection string (masked for display)
            ConnectionStringMasked = MaskConnectionString(_configuration.GetConnectionString("RVToolsDW") ?? "")
        };

        return Task.FromResult(settings);
    }

    public async Task<(bool Success, string? Error)> UpdateAppSettingsAsync(AppSettingsViewModel settings)
    {
        try
        {
            var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");

            // Validate file exists
            if (!File.Exists(appSettingsPath))
            {
                return (false, "appsettings.json file not found");
            }

            // Check if file is writable
            var fileInfo = new FileInfo(appSettingsPath);
            if (fileInfo.IsReadOnly)
            {
                return (false, "appsettings.json is read-only");
            }

            // Read current JSON
            var json = await File.ReadAllTextAsync(appSettingsPath);
            var jsonObj = JsonNode.Parse(json);
            if (jsonObj == null)
            {
                return (false, "Invalid JSON in appsettings.json");
            }

            // Update UI section
            EnsureSection(jsonObj, "Ui");
            jsonObj["Ui"]!["ApplicationTitle"] = settings.ApplicationTitle;
            jsonObj["Ui"]!["LogoPath"] = settings.LogoPath;

            // Update Caching section
            EnsureSection(jsonObj, "Caching");
            jsonObj["Caching"]!["FilterCacheMinutes"] = settings.FilterCacheMinutes;

            // Update WebLogging section
            EnsureSection(jsonObj, "WebLogging");
            jsonObj["WebLogging"]!["Enabled"] = settings.LoggingEnabled;
            jsonObj["WebLogging"]!["MinimumLevel"] = settings.LoggingMinimumLevel;
            jsonObj["WebLogging"]!["RetentionDays"] = settings.LogRetentionDays;

            // Update nested logging sections
            EnsureSection(jsonObj["WebLogging"]!, "DatabaseLogging");
            jsonObj["WebLogging"]!["DatabaseLogging"]!["Enabled"] = settings.DatabaseLoggingEnabled;
            jsonObj["WebLogging"]!["DatabaseLogging"]!["MinimumLevel"] = settings.DatabaseLoggingMinimumLevel;

            EnsureSection(jsonObj["WebLogging"]!, "ConsoleLogging");
            jsonObj["WebLogging"]!["ConsoleLogging"]!["Enabled"] = settings.ConsoleLoggingEnabled;
            jsonObj["WebLogging"]!["ConsoleLogging"]!["MinimumLevel"] = settings.ConsoleLoggingMinimumLevel;

            // Write back with formatting
            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(appSettingsPath, jsonObj.ToJsonString(options));

            _logger.LogWarning("appsettings.json modified via Admin UI. Application restart may be required for some settings.");

            return (true, null);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogError("Unauthorized access when trying to update appsettings.json");
            return (false, "Access denied. The application does not have permission to modify appsettings.json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update appsettings.json");
            return (false, ex.Message);
        }
    }

    private static void EnsureSection(JsonNode parent, string sectionName)
    {
        if (parent[sectionName] == null)
        {
            parent[sectionName] = new JsonObject();
        }
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "(not configured)";
        }

        // Parse connection string and mask sensitive values
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var maskedParts = new List<string>();

        foreach (var part in parts)
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim().ToLowerInvariant();
                var value = keyValue[1].Trim();

                // Mask password-related values
                if (key.Contains("password") || key.Contains("pwd"))
                {
                    maskedParts.Add($"{keyValue[0]}=********");
                }
                else
                {
                    maskedParts.Add(part);
                }
            }
            else
            {
                maskedParts.Add(part);
            }
        }

        return string.Join("; ", maskedParts);
    }
}
