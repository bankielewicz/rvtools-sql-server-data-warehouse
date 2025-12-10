using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the Host Inventory report.
/// </summary>
public class HostInventoryViewModel
{
    public HostInventoryFilter Filter { get; set; } = new();
    public IEnumerable<HostInventoryItem> Items { get; set; } = Enumerable.Empty<HostInventoryItem>();
    public IEnumerable<FilterOptionDto> Datacenters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> Clusters { get; set; } = Enumerable.Empty<FilterOptionDto>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Host Inventory report.
/// </summary>
public class HostInventoryFilter
{
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single host record from the vw_Host_Inventory view.
/// </summary>
public class HostInventoryItem
{
    // Identity
    public string Host { get; set; } = string.Empty;
    public string? UUID { get; set; }
    public string? VI_SDK_Server { get; set; }

    // Location
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }

    // Status
    public string? Config_status { get; set; }
    public bool? in_Maintenance_Mode { get; set; }
    public bool? in_Quarantine_Mode { get; set; }

    // CPU
    public string? CPU_Model { get; set; }
    public int? Speed { get; set; }
    public int? Num_CPU { get; set; }
    public int? Cores_per_CPU { get; set; }
    public int? Num_Cores { get; set; }
    public bool? HT_Available { get; set; }
    public bool? HT_Active { get; set; }

    // Memory (MiB)
    public long? Num_Memory { get; set; }

    // Adapters
    public int? Num_NICs { get; set; }
    public int? Num_HBAs { get; set; }

    // VMs
    public int? Num_VMs { get; set; }
    public int? Num_vCPUs { get; set; }
    public decimal? vCPUs_per_Core { get; set; }
    public long? vRAM { get; set; }

    // Software
    public string? ESX_Version { get; set; }
    public string? Current_EVC { get; set; }
    public string? Max_EVC { get; set; }

    // Hardware
    public string? Vendor { get; set; }
    public string? Model { get; set; }
    public string? Serial_number { get; set; }
    public string? Service_tag { get; set; }
    public string? BIOS_Version { get; set; }
    public DateTime? BIOS_Date { get; set; }

    // Time
    public DateTime? Boot_time { get; set; }
    public string? Time_Zone_Name { get; set; }

    // Certificate
    public DateTime? Certificate_Expiry_Date { get; set; }
    public string? Certificate_Status { get; set; }
}
