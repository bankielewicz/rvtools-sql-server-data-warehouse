using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// ESXi host summary from /api/vcenter/host endpoint.
/// Maps to RVTools vHost sheet.
/// </summary>
public class HostInfo
{
    /// <summary>
    /// Host identifier (e.g., "host-123").
    /// </summary>
    [JsonPropertyName("host")]
    public string HostId { get; set; } = string.Empty;

    /// <summary>
    /// Host display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Connection state (CONNECTED, DISCONNECTED, NOT_RESPONDING).
    /// </summary>
    [JsonPropertyName("connection_state")]
    public string ConnectionState { get; set; } = string.Empty;

    /// <summary>
    /// Power state (POWERED_ON, POWERED_OFF, STANDBY).
    /// </summary>
    [JsonPropertyName("power_state")]
    public string PowerState { get; set; } = string.Empty;
}

/// <summary>
/// Detailed host information from /api/vcenter/host/{host} endpoint.
/// </summary>
public class HostDetail
{
    /// <summary>
    /// Host name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Connection state.
    /// </summary>
    [JsonPropertyName("connection_state")]
    public string? ConnectionState { get; set; }

    /// <summary>
    /// Power state.
    /// </summary>
    [JsonPropertyName("power_state")]
    public string? PowerState { get; set; }
}
