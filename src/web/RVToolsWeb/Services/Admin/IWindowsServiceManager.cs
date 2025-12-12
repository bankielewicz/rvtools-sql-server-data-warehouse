using RVToolsWeb.Models.ViewModels.Admin;

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Interface for managing Windows Services with impersonation support.
/// </summary>
public interface IWindowsServiceManager
{
    /// <summary>
    /// Gets the current status of the Windows Service.
    /// </summary>
    WindowsServiceStatus GetServiceStatus();

    /// <summary>
    /// Starts the Windows Service using impersonation.
    /// </summary>
    Task<ServiceOperationResult> StartServiceAsync();

    /// <summary>
    /// Stops the Windows Service using impersonation.
    /// </summary>
    Task<ServiceOperationResult> StopServiceAsync();


    /// <summary>
    /// Checks if the current user is a local administrator.
    /// </summary>
    bool IsCurrentUserLocalAdmin();

    /// <summary>
    /// Gets the configured executable path for the service.
    /// </summary>
    string GetExecutablePath();
}
