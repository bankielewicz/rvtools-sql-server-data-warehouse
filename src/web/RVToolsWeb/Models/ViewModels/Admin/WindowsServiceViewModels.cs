namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// Windows Service state from ServiceController.
/// </summary>
public enum WindowsServiceState
{
    Unknown,
    NotInstalled,
    Stopped,
    StartPending,
    StopPending,
    Running,
    ContinuePending,
    PausePending,
    Paused
}

/// <summary>
/// Status of the Windows Service from ServiceController.
/// </summary>
public class WindowsServiceStatus
{
    public string ServiceName { get; set; } = "RVToolsImportService";
    public string DisplayName { get; set; } = "RVTools Import Service";
    public WindowsServiceState State { get; set; } = WindowsServiceState.Unknown;
    public string StateDisplay { get; set; } = string.Empty;
    public bool CanStart { get; set; }
    public bool CanStop { get; set; }
    public bool CanInstall { get; set; }
    public bool CanUninstall { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExecutablePath { get; set; }

    public string StateBadgeClass => State switch
    {
        WindowsServiceState.Running => "bg-success",
        WindowsServiceState.Stopped => "bg-warning",
        WindowsServiceState.NotInstalled => "bg-secondary",
        WindowsServiceState.StartPending or WindowsServiceState.StopPending => "bg-info",
        _ => "bg-danger"
    };

    public string StateIcon => State switch
    {
        WindowsServiceState.Running => "bi-check-circle-fill",
        WindowsServiceState.Stopped => "bi-pause-circle-fill",
        WindowsServiceState.NotInstalled => "bi-x-circle",
        WindowsServiceState.StartPending or WindowsServiceState.StopPending => "bi-hourglass-split",
        _ => "bi-question-circle"
    };
}

/// <summary>
/// Result of a service operation.
/// </summary>
public class ServiceOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }

    public static ServiceOperationResult Ok(string message) =>
        new() { Success = true, Message = message };

    public static ServiceOperationResult Fail(string message, string? details = null) =>
        new() { Success = false, Message = message, ErrorDetails = details };
}
