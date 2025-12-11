using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Service interface for managing Config.Settings database table.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Get all settings from Config.Settings.
    /// </summary>
    Task<IEnumerable<GeneralSettingViewModel>> GetAllSettingsAsync();

    /// <summary>
    /// Get a single setting by name.
    /// </summary>
    Task<GeneralSettingViewModel?> GetSettingByNameAsync(string settingName);

    /// <summary>
    /// Update a setting value.
    /// </summary>
    Task<bool> UpdateSettingAsync(string settingName, string settingValue);
}
