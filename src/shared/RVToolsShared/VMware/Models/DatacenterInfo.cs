using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Datacenter summary from /api/vcenter/datacenter endpoint.
/// </summary>
public class DatacenterInfo
{
    /// <summary>
    /// Datacenter identifier (e.g., "datacenter-123").
    /// </summary>
    [JsonPropertyName("datacenter")]
    public string DatacenterId { get; set; } = string.Empty;

    /// <summary>
    /// Datacenter display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
