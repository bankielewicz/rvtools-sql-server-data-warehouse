using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// Virtual machine summary from /api/vcenter/vm endpoint.
/// Maps to RVTools vInfo sheet.
/// </summary>
public class VmInfo
{
    /// <summary>
    /// VM identifier (e.g., "vm-123").
    /// </summary>
    [JsonPropertyName("vm")]
    public string VmId { get; set; } = string.Empty;

    /// <summary>
    /// VM display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Power state (POWERED_ON, POWERED_OFF, SUSPENDED).
    /// </summary>
    [JsonPropertyName("power_state")]
    public string PowerState { get; set; } = string.Empty;

    /// <summary>
    /// Number of CPUs.
    /// </summary>
    [JsonPropertyName("cpu_count")]
    public int? CpuCount { get; set; }

    /// <summary>
    /// Memory size in megabytes.
    /// </summary>
    [JsonPropertyName("memory_size_MiB")]
    public long? MemorySizeMiB { get; set; }
}

/// <summary>
/// Detailed VM information from /api/vcenter/vm/{vm} endpoint.
/// </summary>
public class VmDetail
{
    /// <summary>
    /// VM identity information.
    /// </summary>
    [JsonPropertyName("identity")]
    public VmIdentity? Identity { get; set; }

    /// <summary>
    /// VM guest OS information.
    /// </summary>
    [JsonPropertyName("guest_OS")]
    public string? GuestOS { get; set; }

    /// <summary>
    /// VM hardware information.
    /// </summary>
    [JsonPropertyName("hardware")]
    public VmHardware? Hardware { get; set; }

    /// <summary>
    /// VM guest information (requires VMware Tools).
    /// </summary>
    [JsonPropertyName("guest")]
    public VmGuest? Guest { get; set; }

    /// <summary>
    /// CPU configuration.
    /// </summary>
    [JsonPropertyName("cpu")]
    public VmCpu? Cpu { get; set; }

    /// <summary>
    /// Memory configuration.
    /// </summary>
    [JsonPropertyName("memory")]
    public VmMemory? Memory { get; set; }

    /// <summary>
    /// Power state.
    /// </summary>
    [JsonPropertyName("power_state")]
    public string? PowerState { get; set; }

    /// <summary>
    /// Boot configuration.
    /// </summary>
    [JsonPropertyName("boot")]
    public VmBoot? Boot { get; set; }
}

/// <summary>
/// VM identity information.
/// </summary>
public class VmIdentity
{
    /// <summary>
    /// VM name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// VM BIOS UUID.
    /// </summary>
    [JsonPropertyName("bios_uuid")]
    public string? BiosUuid { get; set; }

    /// <summary>
    /// VM instance UUID.
    /// </summary>
    [JsonPropertyName("instance_uuid")]
    public string? InstanceUuid { get; set; }
}

/// <summary>
/// VM hardware configuration.
/// </summary>
public class VmHardware
{
    /// <summary>
    /// Hardware version (e.g., "vmx-19").
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Upgrade policy.
    /// </summary>
    [JsonPropertyName("upgrade_policy")]
    public string? UpgradePolicy { get; set; }

    /// <summary>
    /// Upgrade status.
    /// </summary>
    [JsonPropertyName("upgrade_status")]
    public string? UpgradeStatus { get; set; }
}

/// <summary>
/// VM guest information (from VMware Tools).
/// </summary>
public class VmGuest
{
    /// <summary>
    /// Guest OS full name.
    /// </summary>
    [JsonPropertyName("os_full_name")]
    public string? OsFullName { get; set; }

    /// <summary>
    /// Guest hostname.
    /// </summary>
    [JsonPropertyName("host_name")]
    public string? HostName { get; set; }

    /// <summary>
    /// Guest IP address.
    /// </summary>
    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }
}

/// <summary>
/// VM CPU configuration.
/// </summary>
public class VmCpu
{
    /// <summary>
    /// Number of CPU cores.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// Number of cores per socket.
    /// </summary>
    [JsonPropertyName("cores_per_socket")]
    public int CoresPerSocket { get; set; }

    /// <summary>
    /// Hot add enabled.
    /// </summary>
    [JsonPropertyName("hot_add_enabled")]
    public bool HotAddEnabled { get; set; }

    /// <summary>
    /// Hot remove enabled.
    /// </summary>
    [JsonPropertyName("hot_remove_enabled")]
    public bool HotRemoveEnabled { get; set; }
}

/// <summary>
/// VM memory configuration.
/// </summary>
public class VmMemory
{
    /// <summary>
    /// Memory size in megabytes.
    /// </summary>
    [JsonPropertyName("size_MiB")]
    public long SizeMiB { get; set; }

    /// <summary>
    /// Hot add enabled.
    /// </summary>
    [JsonPropertyName("hot_add_enabled")]
    public bool HotAddEnabled { get; set; }
}

/// <summary>
/// VM boot configuration.
/// </summary>
public class VmBoot
{
    /// <summary>
    /// Boot type (BIOS or EFI).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Whether EFI secure boot is enabled.
    /// </summary>
    [JsonPropertyName("efi_legacy_boot")]
    public bool? EfiLegacyBoot { get; set; }

    /// <summary>
    /// Boot delay in milliseconds.
    /// </summary>
    [JsonPropertyName("delay")]
    public long? Delay { get; set; }

    /// <summary>
    /// Enter setup on boot.
    /// </summary>
    [JsonPropertyName("enter_setup_mode")]
    public bool? EnterSetupMode { get; set; }
}
