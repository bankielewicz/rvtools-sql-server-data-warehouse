namespace RVToolsService.Services;

/// <summary>
/// Service for managing service health status and heartbeats.
/// </summary>
public interface IServiceHealthService
{
    /// <summary>
    /// Updates the heartbeat timestamp in the database.
    /// </summary>
    Task UpdateHeartbeatAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the service status in the database.
    /// </summary>
    /// <param name="status">Status value: Running, Stopped, Error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current service status from the database.
    /// </summary>
    Task<ServiceHealthInfo?> GetStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service health information.
/// </summary>
public class ServiceHealthInfo
{
    public string ServiceName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastHeartbeat { get; set; }
    public string? ServiceVersion { get; set; }
    public int ActiveJobs { get; set; }
    public int QueuedJobs { get; set; }
}
