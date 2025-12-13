using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Network summary from /api/vcenter/network endpoint.
/// Maps to RVTools vNetwork sheet (network-level, not VM NICs).
/// </summary>
public class NetworkInfo
{
    /// <summary>
    /// Network identifier (e.g., "network-123" or "dvportgroup-456").
    /// </summary>
    [JsonPropertyName("network")]
    public string NetworkId { get; set; } = string.Empty;

    /// <summary>
    /// Network display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Network type (STANDARD_PORTGROUP, DISTRIBUTED_PORTGROUP, OPAQUE_NETWORK).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
