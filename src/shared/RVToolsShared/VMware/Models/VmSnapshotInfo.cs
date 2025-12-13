using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// VM snapshot information from /api/vcenter/vm/{vm}/snapshots endpoint.
/// Maps to RVTools vSnapshot sheet.
/// </summary>
public class VmSnapshotInfo
{
    /// <summary>
    /// Current snapshot ID (if any).
    /// </summary>
    [JsonPropertyName("current_snapshot")]
    public string? CurrentSnapshot { get; set; }

    /// <summary>
    /// List of all snapshots.
    /// </summary>
    [JsonPropertyName("snapshots")]
    public List<SnapshotEntry>? Snapshots { get; set; }
}

/// <summary>
/// Individual snapshot entry.
/// </summary>
public class SnapshotEntry
{
    /// <summary>
    /// Snapshot identifier.
    /// </summary>
    [JsonPropertyName("snapshot")]
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Snapshot description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// When the snapshot was created.
    /// </summary>
    [JsonPropertyName("create_time")]
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// Power state when snapshot was taken.
    /// </summary>
    [JsonPropertyName("power_state")]
    public string? PowerState { get; set; }

    /// <summary>
    /// Whether this snapshot is revertable.
    /// </summary>
    [JsonPropertyName("revertable")]
    public bool? Revertable { get; set; }

    /// <summary>
    /// Size of the snapshot data in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long? Size { get; set; }

    /// <summary>
    /// Parent snapshot ID.
    /// </summary>
    [JsonPropertyName("parent")]
    public string? Parent { get; set; }

    /// <summary>
    /// Child snapshots.
    /// </summary>
    [JsonPropertyName("children")]
    public List<string>? Children { get; set; }
}
