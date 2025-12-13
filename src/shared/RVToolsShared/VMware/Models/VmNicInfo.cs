using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// VM NIC information from /api/vcenter/vm/{vm}/hardware/ethernet endpoint.
/// Maps to VM portion of RVTools vNetwork sheet.
/// </summary>
public class VmNicInfo
{
    /// <summary>
    /// NIC key/identifier.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// NIC label.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// NIC type (E1000, E1000E, VMXNET3, etc.).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// MAC address type (MANUAL, GENERATED, ASSIGNED).
    /// </summary>
    [JsonPropertyName("mac_type")]
    public string? MacType { get; set; }

    /// <summary>
    /// MAC address.
    /// </summary>
    [JsonPropertyName("mac_address")]
    public string? MacAddress { get; set; }

    /// <summary>
    /// Whether the NIC starts connected.
    /// </summary>
    [JsonPropertyName("start_connected")]
    public bool? StartConnected { get; set; }

    /// <summary>
    /// Whether the NIC allows guest control.
    /// </summary>
    [JsonPropertyName("allow_guest_control")]
    public bool? AllowGuestControl { get; set; }

    /// <summary>
    /// Backing information (network connection).
    /// </summary>
    [JsonPropertyName("backing")]
    public VmNicBacking? Backing { get; set; }

    /// <summary>
    /// Connection state.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>
    /// UPT compatibility enabled.
    /// </summary>
    [JsonPropertyName("upt_compatibility_enabled")]
    public bool? UptCompatibilityEnabled { get; set; }

    /// <summary>
    /// Wake-on-LAN enabled.
    /// </summary>
    [JsonPropertyName("wake_on_lan_enabled")]
    public bool? WakeOnLanEnabled { get; set; }
}

/// <summary>
/// NIC backing information.
/// </summary>
public class VmNicBacking
{
    /// <summary>
    /// Backing type (STANDARD_PORTGROUP, DISTRIBUTED_PORTGROUP, etc.).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Network identifier.
    /// </summary>
    [JsonPropertyName("network")]
    public string? Network { get; set; }

    /// <summary>
    /// Network name (for standard portgroups).
    /// </summary>
    [JsonPropertyName("network_name")]
    public string? NetworkName { get; set; }

    /// <summary>
    /// Distributed switch UUID (for distributed portgroups).
    /// </summary>
    [JsonPropertyName("distributed_switch_uuid")]
    public string? DistributedSwitchUuid { get; set; }

    /// <summary>
    /// Distributed port (for distributed portgroups).
    /// </summary>
    [JsonPropertyName("distributed_port")]
    public string? DistributedPort { get; set; }

    /// <summary>
    /// Connection cookie (for distributed portgroups).
    /// </summary>
    [JsonPropertyName("connection_cookie")]
    public int? ConnectionCookie { get; set; }
}
