using RVToolsWeb.Models.DTOs;

namespace RVToolsWeb.Models.ViewModels.Health;

/// <summary>
/// View model for the Certificate Expiration report.
/// </summary>
public class CertificateExpirationViewModel
{
    public CertificateExpirationFilter Filter { get; set; } = new();
    public IEnumerable<CertificateExpirationItem> Items { get; set; } = Enumerable.Empty<CertificateExpirationItem>();
    public IEnumerable<FilterOptionDto> VISdkServers { get; set; } = Enumerable.Empty<FilterOptionDto>();
}

/// <summary>
/// Filter parameters for the Certificate Expiration report.
/// </summary>
public class CertificateExpirationFilter
{
    public string? VI_SDK_Server { get; set; }
    public bool IncludeValid { get; set; } = true;
}

/// <summary>
/// Single certificate record from the vw_Health_Certificate_Expiration view.
/// </summary>
public class CertificateExpirationItem
{
    public string? HostName { get; set; }
    public string? VI_SDK_Server { get; set; }
    public string? Datacenter { get; set; }
    public string? Cluster { get; set; }
    public string? Certificate_Issuer { get; set; }
    public string? Certificate_Subject { get; set; }
    public string? Certificate_Status { get; set; }
    public DateTime? Certificate_Start_Date { get; set; }
    public DateTime? Certificate_Expiry_Date { get; set; }
    public int? Days_Until_Expiration { get; set; }
    public string? Expiration_Status { get; set; }
    public string? ESX_Version { get; set; }
}
