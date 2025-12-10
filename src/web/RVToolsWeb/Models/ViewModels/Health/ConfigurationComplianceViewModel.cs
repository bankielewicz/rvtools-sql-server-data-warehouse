using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the Configuration Compliance report.
/// </summary>
public class ConfigurationComplianceViewModel
{
    public ConfigurationComplianceFilter Filter { get; set; } = new();
    public IEnumerable<ConfigurationComplianceItem> Items { get; set; } = Enumerable.Empty<ConfigurationComplianceItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();

    // Summary metrics
    public int CompliantCount => Items.Count(x => x.Overall_Compliance_Status == "Compliant");
    public int NonCompliantCount => Items.Count(x => x.Overall_Compliance_Status == "Non-Compliant");
    public decimal CompliancePercent => Items.Any() ? (CompliantCount * 100m / Items.Count()) : 0;
}

/// <summary>
/// Filter parameters for the Configuration Compliance report.
/// </summary>
public class ConfigurationComplianceFilter
{
    public string? VI_SDK_Server { get; set; }
    public bool ShowNonCompliantOnly { get; set; }
}

/// <summary>
/// Single VM compliance record from the vw_Health_Configuration_Compliance view.
/// </summary>
public class ConfigurationComplianceItem
{
    // VM Identity
    public string? VM { get; set; }
    public string? VM_UUID { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Powerstate { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Host { get; set; }
    public string? Resource_pool { get; set; }

    // OS
    public string? OS_according_to_the_VMware_Tools { get; set; }

    // CPU Metrics
    public int? CPU_Count { get; set; }
    public int? Host_Physical_Cores { get; set; }
    public decimal? vCPU_to_Core_Ratio { get; set; }

    // Memory Metrics
    public long? Memory_Allocated_MiB { get; set; }
    public long? Memory_Reservation_MiB { get; set; }
    public decimal? Memory_Reservation_Percent { get; set; }

    // Boot Settings
    public int? Boot_Delay_Seconds { get; set; }

    // Tools Status
    public string? Tools_Status { get; set; }
    public string? Tools_Version { get; set; }
    public string? Tools_Upgradeable { get; set; }

    // Compliance Checks
    public int? vCPU_Ratio_Compliant { get; set; }
    public int? Memory_Reservation_Compliant { get; set; }
    public int? Boot_Delay_Compliant { get; set; }
    public int? Tools_Compliant { get; set; }

    // Overall
    public string? Overall_Compliance_Status { get; set; }
}
