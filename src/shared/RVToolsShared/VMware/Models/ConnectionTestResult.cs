namespace RVToolsShared.VMware.Models;

/// <summary>
/// Result of a connection test to vCenter.
/// </summary>
public class ConnectionTestResult
{
    /// <summary>
    /// Whether the connection test was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if connection failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// vCenter version information.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// vCenter build number.
    /// </summary>
    public string? Build { get; set; }

    /// <summary>
    /// vCenter product name (e.g., "VMware vCenter Server").
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// vCenter instance UUID.
    /// </summary>
    public string? InstanceUuid { get; set; }

    /// <summary>
    /// Time taken to complete the connection test in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Timestamp when the test was performed (UTC).
    /// </summary>
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ConnectionTestResult Successful(string? version = null, string? build = null, string? productName = null, string? instanceUuid = null, long responseTimeMs = 0)
    {
        return new ConnectionTestResult
        {
            Success = true,
            Version = version,
            Build = build,
            ProductName = productName,
            InstanceUuid = instanceUuid,
            ResponseTimeMs = responseTimeMs
        };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static ConnectionTestResult Failed(string errorMessage, long responseTimeMs = 0)
    {
        return new ConnectionTestResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ResponseTimeMs = responseTimeMs
        };
    }
}
