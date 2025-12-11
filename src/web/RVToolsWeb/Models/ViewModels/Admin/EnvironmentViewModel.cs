namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// View model for Environment tab in Settings
/// </summary>
public class EnvironmentViewModel
{
    public string CurrentEnvironment { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; }
    public bool IsProduction { get; set; }
    public string ContentRootPath { get; set; } = string.Empty;
    public string WebRootPath { get; set; } = string.Empty;
    public string WebConfigPath { get; set; } = string.Empty;
    public bool CanModifyWebConfig { get; set; }
}
