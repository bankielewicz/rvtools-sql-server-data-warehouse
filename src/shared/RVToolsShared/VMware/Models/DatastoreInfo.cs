using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Datastore summary from /api/vcenter/datastore endpoint.
/// Maps to RVTools vDatastore sheet.
/// </summary>
public class DatastoreInfo
{
    /// <summary>
    /// Datastore identifier (e.g., "datastore-123").
    /// </summary>
    [JsonPropertyName("datastore")]
    public string DatastoreId { get; set; } = string.Empty;

    /// <summary>
    /// Datastore display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Datastore type (VMFS, NFS, VSAN, VVOL).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Free space in bytes.
    /// </summary>
    [JsonPropertyName("free_space")]
    public long? FreeSpace { get; set; }

    /// <summary>
    /// Total capacity in bytes.
    /// </summary>
    [JsonPropertyName("capacity")]
    public long? Capacity { get; set; }
}
