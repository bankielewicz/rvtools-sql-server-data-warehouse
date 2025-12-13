using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Resource pool summary from /api/vcenter/resource-pool endpoint.
/// Maps to RVTools vRP sheet.
/// </summary>
public class ResourcePoolInfo
{
    /// <summary>
    /// Resource pool identifier (e.g., "resgroup-123").
    /// </summary>
    [JsonPropertyName("resource_pool")]
    public string ResourcePoolId { get; set; } = string.Empty;

    /// <summary>
    /// Resource pool display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
