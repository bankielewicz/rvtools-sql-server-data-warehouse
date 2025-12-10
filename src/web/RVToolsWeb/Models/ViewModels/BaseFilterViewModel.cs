namespace RVToolsWeb.Models.ViewModels;

/// <summary>
/// Base class for filter view models with common filter properties.
/// </summary>
public abstract class BaseFilterViewModel
{
    /// <summary>
    /// Filter by vCenter Server (VI_SDK_Server column).
    /// </summary>
    public string? VI_SDK_Server { get; set; }
}
