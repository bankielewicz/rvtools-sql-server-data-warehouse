using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service interface for managing appsettings.json configuration.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Get current application settings from appsettings.json.
    /// </summary>
    Task<AppSettingsViewModel> GetAppSettingsAsync();

    /// <summary>
    /// Update application settings in appsettings.json file.
    /// </summary>
    /// <returns>Tuple with success flag and optional error message.</returns>
    Task<(bool Success, string? Error)> UpdateAppSettingsAsync(AppSettingsViewModel settings);
}
