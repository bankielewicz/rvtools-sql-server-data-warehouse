using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Cluster summary from /api/vcenter/cluster endpoint.
/// Maps to RVTools vCluster sheet.
/// </summary>
public class ClusterInfo
{
    /// <summary>
    /// Cluster identifier (e.g., "domain-c123").
    /// </summary>
    [JsonPropertyName("cluster")]
    public string ClusterId { get; set; } = string.Empty;

    /// <summary>
    /// Cluster display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether HA is enabled.
    /// </summary>
    [JsonPropertyName("ha_enabled")]
    public bool? HaEnabled { get; set; }

    /// <summary>
    /// Whether DRS is enabled.
    /// </summary>
    [JsonPropertyName("drs_enabled")]
    public bool? DrsEnabled { get; set; }
}
