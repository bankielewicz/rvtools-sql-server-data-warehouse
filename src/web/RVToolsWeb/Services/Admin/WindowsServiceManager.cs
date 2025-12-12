using System.Diagnostics;
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
    private readonly string _executablePath;
    private readonly ILogger<WindowsServiceManager> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WindowsServiceManager(
        IConfiguration configuration,
        ILogger<WindowsServiceManager> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _serviceName = configuration["WindowsService:ServiceName"] ?? "RVToolsImportService";
        _displayName = configuration["WindowsService:DisplayName"] ?? "RVTools Import Service";
        _executablePath = configuration["WindowsService:ExecutablePath"]
            ?? @"C:\Services\RVToolsService\RVToolsService.exe";
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetExecutablePath() => _executablePath;

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
                ErrorMessage = "Windows Service management is only available on Windows",
                ExecutablePath = _executablePath
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
                CanStop = sc.Status == ServiceControllerStatus.Running,
                CanInstall = false,
                CanUninstall = sc.Status == ServiceControllerStatus.Stopped,
                ExecutablePath = _executablePath
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
                CanStop = false,
                CanInstall = true,
                CanUninstall = false,
                ExecutablePath = _executablePath
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
                ErrorMessage = ex.Message,
                ExecutablePath = _executablePath
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

    public async Task<ServiceOperationResult> InstallServiceAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ServiceOperationResult.Fail("Windows Service management is only available on Windows");
        }

        _logger.LogInformation("Installing service {ServiceName} from {Path}", _serviceName, _executablePath);

        // Verify executable exists
        if (!File.Exists(_executablePath))
        {
            return ServiceOperationResult.Fail(
                "Service executable not found",
                $"Expected path: {_executablePath}");
        }

        return await RunImpersonatedAsync(() =>
        {
            try
            {
                var arguments = $"create \"{_serviceName}\" binPath= \"{_executablePath}\" DisplayName= \"{_displayName}\" start= auto";

                var result = RunScCommand(arguments);
                if (result.Success)
                {
                    _logger.LogInformation("Service {ServiceName} installed successfully", _serviceName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to install service {ServiceName}", _serviceName);
                return ServiceOperationResult.Fail("Failed to install service", ex.Message);
            }
        });
    }

    public async Task<ServiceOperationResult> UninstallServiceAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return ServiceOperationResult.Fail("Windows Service management is only available on Windows");
        }

        _logger.LogInformation("Uninstalling service {ServiceName}", _serviceName);

        return await RunImpersonatedAsync(() =>
        {
            try
            {
                // First, ensure service is stopped
                try
                {
                    using var sc = new ServiceController(_serviceName);
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
                catch (InvalidOperationException)
                {
                    // Service doesn't exist - that's fine
                    return ServiceOperationResult.Ok("Service was not installed");
                }

                var result = RunScCommand($"delete \"{_serviceName}\"");
                if (result.Success)
                {
                    _logger.LogInformation("Service {ServiceName} uninstalled successfully", _serviceName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to uninstall service {ServiceName}", _serviceName);
                return ServiceOperationResult.Fail("Failed to uninstall service", ex.Message);
            }
        });
    }

    public bool IsCurrentUserLocalAdmin()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        var windowsIdentity = _httpContextAccessor.HttpContext?.User?.Identity as WindowsIdentity;
        if (windowsIdentity == null)
        {
            return false;
        }

        var principal = new WindowsPrincipal(windowsIdentity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private async Task<ServiceOperationResult> RunImpersonatedAsync(Func<ServiceOperationResult> action)
    {
        var windowsIdentity = _httpContextAccessor.HttpContext?.User?.Identity as WindowsIdentity;
        if (windowsIdentity == null)
        {
            return ServiceOperationResult.Fail("Windows authentication required");
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

    private static ServiceOperationResult RunScCommand(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return ServiceOperationResult.Fail("Failed to start sc.exe");
        }

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(30000);

        if (process.ExitCode == 0)
        {
            return ServiceOperationResult.Ok(stdout.Trim());
        }

        var errorMessage = !string.IsNullOrEmpty(stderr) ? stderr : stdout;
        return ServiceOperationResult.Fail($"sc.exe returned exit code {process.ExitCode}", errorMessage.Trim());
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
