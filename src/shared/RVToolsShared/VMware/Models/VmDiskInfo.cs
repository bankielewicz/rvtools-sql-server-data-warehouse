using System.Text.Json.Serialization;

namespace RVToolsShared.VMware.Models;

/// <summary>
/// VM disk information from /api/vcenter/vm/{vm}/hardware/disk endpoint.
/// Maps to RVTools vDisk sheet.
/// </summary>
public class VmDiskInfo
{
    /// <summary>
    /// Disk key/identifier.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Disk label.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Disk type (IDE, SCSI, SATA, NVME).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Capacity in bytes.
    /// </summary>
    [JsonPropertyName("capacity")]
    public long? Capacity { get; set; }

    /// <summary>
    /// Backing information (where the disk is stored).
    /// </summary>
    [JsonPropertyName("backing")]
    public VmDiskBacking? Backing { get; set; }

    /// <summary>
    /// SCSI configuration (if applicable).
    /// </summary>
    [JsonPropertyName("scsi")]
    public VmDiskScsi? Scsi { get; set; }

    /// <summary>
    /// IDE configuration (if applicable).
    /// </summary>
    [JsonPropertyName("ide")]
    public VmDiskIde? Ide { get; set; }

    /// <summary>
    /// SATA configuration (if applicable).
    /// </summary>
    [JsonPropertyName("sata")]
    public VmDiskSata? Sata { get; set; }
}

/// <summary>
/// Disk backing information.
/// </summary>
public class VmDiskBacking
{
    /// <summary>
    /// Backing type (VMDK_FILE).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// VMDK file path.
    /// </summary>
    [JsonPropertyName("vmdk_file")]
    public string? VmdkFile { get; set; }
}

/// <summary>
/// SCSI disk configuration.
/// </summary>
public class VmDiskScsi
{
    /// <summary>
    /// SCSI bus number.
    /// </summary>
    [JsonPropertyName("bus")]
    public int? Bus { get; set; }

    /// <summary>
    /// SCSI unit number.
    /// </summary>
    [JsonPropertyName("unit")]
    public int? Unit { get; set; }
}

/// <summary>
/// IDE disk configuration.
/// </summary>
public class VmDiskIde
{
    /// <summary>
    /// IDE primary/secondary.
    /// </summary>
    [JsonPropertyName("primary")]
    public bool? Primary { get; set; }

    /// <summary>
    /// IDE master/slave.
    /// </summary>
    [JsonPropertyName("master")]
    public bool? Master { get; set; }
}

/// <summary>
/// SATA disk configuration.
/// </summary>
public class VmDiskSata
{
    /// <summary>
    /// SATA bus number.
    /// </summary>
    [JsonPropertyName("bus")]
    public int? Bus { get; set; }

    /// <summary>
    /// SATA unit number.
    /// </summary>
    [JsonPropertyName("unit")]
    public int? Unit { get; set; }
}
