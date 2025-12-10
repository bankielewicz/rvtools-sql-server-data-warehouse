using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Inventory;

/// <summary>
/// View model for the License Compliance report.
/// </summary>
public class LicenseComplianceViewModel
{
    public LicenseComplianceFilter Filter { get; set; } = new();
    public IEnumerable<LicenseComplianceItem> Items { get; set; } = Enumerable.Empty<LicenseComplianceItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the License Compliance report.
/// </summary>
public class LicenseComplianceFilter
{
    public string? VI_SDK_Server { get; set; }
}

/// <summary>
/// Single license record from the vw_Inventory_License_Compliance view.
/// </summary>
public class LicenseComplianceItem
{
    // License Details
    public string? LicenseName { get; set; }
    public string? LicenseKey { get; set; }
    public string? Labels { get; set; }
    public string? Cost_Unit { get; set; }

    // Usage
    public int? Total_Licenses { get; set; }
    public int? Used_Licenses { get; set; }
    public int? Available_Licenses { get; set; }
    public decimal? Usage_Percent { get; set; }

    // Expiration
    public DateTime? Expiration_Date { get; set; }
    public int? Days_Until_Expiration { get; set; }

    // Compliance Status
    public string? Compliance_Status { get; set; }

    // Features
    public string? Features { get; set; }

    // Source
    public string? VI_SDK_Server { get; set; }
}
