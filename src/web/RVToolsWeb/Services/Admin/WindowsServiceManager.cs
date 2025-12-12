using System.Security.Principal;
using System.ServiceProcess;
using RVToolsWeb.Models.ViewModels.Admin;

// Suppress CA1416 warnings - we have runtime checks for Windows via OperatingSystem.IsWindows()
#pragma warning disable CA1416

namespace RVToolsWeb.Services.Admin;

/// <summary>
/// Manages Windows Services using ServiceController and impersonation.
/// </summary>
public class WindowsServiceManager : IWindowsServiceManager
{
    private readonly string _serviceName;
    private readonly string _displayName;
    private readonly ILogger<WindowsServiceManager> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WindowsServiceManager(
        IConfiguration configuration,
        ILogger<WindowsServiceManager> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _serviceName = configuration["WindowsService:ServiceName"] ?? "RVToolsImportService";
        _displayName = configuration["WindowsService:DisplayName"] ?? "RVTools Import Service";
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public WindowsServiceStatus GetServiceStatus()
    {
        // Only works on Windows
        if (!OperatingSystem.IsWindows())
        {
            return new WindowsServiceStatus
            {
                ServiceName = _serviceName,
                DisplayName = _displayName,
                State = WindowsServiceState.Unknown,
                StateDisplay = "Not Available (Non-Windows)",
                ErrorMessage = "Windows Service management is only available on Windows"
            };
        }

        try
        {
            using var sc = new ServiceController(_serviceName);
            var state = MapState(sc.Status);

            return new WindowsServiceStatus
            {
                ServiceName = _serviceName,
                DisplayName = sc.DisplayName,
                State = state,
                StateDisplay = sc.Status.ToString(),
                CanStart = sc.Status == ServiceControllerStatus.Stopped,
                CanStop = sc.Status == ServiceControllerStatus.Running
            };
        }
        catch (InvalidOperationException)
        {
            // Service not installed
            return new WindowsServiceStatus
            {
                ServiceName = _serviceName,
                DisplayName = _displayName,
                State = WindowsServiceState.NotInstalled,
                StateDisplay = "Not Installed",
                CanStart = false,
                CanStop = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service status");
            return new WindowsServiceStatus
            {
                ServiceName = _serviceName,
                DisplayName = _displayName,
                State = WindowsServiceState.Unknown,
                StateDisplay = "Error",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ServiceOperationResult> StartServiceAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ServiceOperationResult.Fail("Windows Service management is only available on Windows");
        }

        _logger.LogInformation("Starting service {ServiceName}", _serviceName);

        return await RunImpersonatedAsync(() =>
        {
            try
            {
                using var sc = new ServiceController(_serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    return ServiceOperationResult.Ok("Service is already running");
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                _logger.LogInformation("Service {ServiceName} started successfully", _serviceName);
                return ServiceOperationResult.Ok("Service started successfully");
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                return ServiceOperationResult.Fail("Service start timed out after 30 seconds");
            }
            catch (InvalidOperationException ex)
            {
                return ServiceOperationResult.Fail("Service not found", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start service {ServiceName}", _serviceName);
                return ServiceOperationResult.Fail("Failed to start service", ex.Message);
            }
        });
    }

    public async Task<ServiceOperationResult> StopServiceAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ServiceOperationResult.Fail("Windows Service management is only available on Windows");
        }

        _logger.LogInformation("Stopping service {ServiceName}", _serviceName);

        return await RunImpersonatedAsync(() =>
        {
            try
            {
                using var sc = new ServiceController(_serviceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    return ServiceOperationResult.Ok("Service is already stopped");
                }

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

                _logger.LogInformation("Service {ServiceName} stopped successfully", _serviceName);
                return ServiceOperationResult.Ok("Service stopped successfully");
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                return ServiceOperationResult.Fail("Service stop timed out after 30 seconds");
            }
            catch (InvalidOperationException ex)
            {
                return ServiceOperationResult.Fail("Service not found", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop service {ServiceName}", _serviceName);
                return ServiceOperationResult.Fail("Failed to stop service", ex.Message);
            }
        });
    }


    public bool IsCurrentUserLocalAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return false;
        }

        // First, check if user has Admin role in the application
        // This allows cookie-based auth users (LDAP/LocalDB) to see service controls
        if (user.IsInRole("Admin"))
        {
            return true;
        }

        // For Windows Integrated Auth, also check local Administrators group
        if (OperatingSystem.IsWindows())
        {
            var windowsIdentity = user.Identity as WindowsIdentity;
            if (windowsIdentity != null)
            {
                var principal = new WindowsPrincipal(windowsIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        return false;
    }

    private async Task<ServiceOperationResult> RunImpersonatedAsync(Func<ServiceOperationResult> action)
    {
        var windowsIdentity = _httpContextAccessor.HttpContext?.User?.Identity as WindowsIdentity;
        if (windowsIdentity == null)
        {
            // Cookie-based auth (LDAP/LocalDB) doesn't provide a Windows token for impersonation
            // Fall back to running under the app pool identity
            _logger.LogWarning("No Windows identity available for impersonation. Running under app pool identity.");

            try
            {
                return await Task.Run(action);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Access denied running under app pool identity: {Error}", ex.Message);
                return ServiceOperationResult.Fail(
                    "Access denied. The IIS application pool identity does not have permission to manage Windows services. " +
                    "Either configure Windows Integrated Authentication or run the app pool as a local administrator.",
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running service operation under app pool identity");
                return ServiceOperationResult.Fail("Operation failed", ex.Message);
            }
        }

        var userName = windowsIdentity.Name;
        _logger.LogInformation("Running service operation as user: {User}", userName);

        try
        {
            return await Task.Run(() =>
                WindowsIdentity.RunImpersonated(windowsIdentity.AccessToken, action));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Access denied for user {User}: {Error}", userName, ex.Message);
            return ServiceOperationResult.Fail(
                "Access denied. Local administrator privileges required.",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during impersonated operation for user {User}", userName);
            return ServiceOperationResult.Fail("Operation failed", ex.Message);
        }
    }


    private static WindowsServiceState MapState(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.Stopped => WindowsServiceState.Stopped,
            ServiceControllerStatus.StartPending => WindowsServiceState.StartPending,
            ServiceControllerStatus.StopPending => WindowsServiceState.StopPending,
            ServiceControllerStatus.Running => WindowsServiceState.Running,
            ServiceControllerStatus.ContinuePending => WindowsServiceState.ContinuePending,
            ServiceControllerStatus.PausePending => WindowsServiceState.PausePending,
            ServiceControllerStatus.Paused => WindowsServiceState.Paused,
            _ => WindowsServiceState.Unknown
        };
    }
}
