namespace RVToolsShared.Models;

/// <summary>
/// DTO representing service health status from Service.ServiceStatus table.
/// </summary>
public class ServiceStatusDto
{
    public int ServiceStatusId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;

    public string Status { get; set; } = "Unknown";
    public DateTime LastHeartbeat { get; set; }
    public string? ServiceVersion { get; set; }

    public int ActiveJobs { get; set; }
    public int QueuedJobs { get; set; }

    /// <summary>
    /// Computed property: true if heartbeat is within last 2 minutes.
    /// </summary>
    public bool IsHealthy => (DateTime.UtcNow - LastHeartbeat).TotalMinutes < 2;
}

/// <summary>
/// Status values for service health.
/// </summary>
public static class ServiceState
{
    public const string Running = "Running";
    public const string Stopped = "Stopped";
    public const string Error = "Error";
    public const string Unknown = "Unknown";
}
