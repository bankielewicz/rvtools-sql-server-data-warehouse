using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Folder summary from /api/vcenter/folder endpoint.
/// </summary>
public class FolderInfo
{
    /// <summary>
    /// Folder identifier (e.g., "group-v123").
    /// </summary>
    [JsonPropertyName("folder")]
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Folder display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Folder type (DATACENTER, DATASTORE, HOST, NETWORK, VIRTUAL_MACHINE).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
