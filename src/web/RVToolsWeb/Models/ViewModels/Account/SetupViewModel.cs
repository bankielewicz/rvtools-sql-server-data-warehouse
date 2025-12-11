namespace RVToolsWeb.Models.ViewModels.Account;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// View model for first-time setup wizard
/// </summary>
public class SetupViewModel
{
    [Required]
    [Display(Name = "Authentication Provider")]
    public string AuthProvider { get; set; } = "LocalDB";

    // LDAP Connection Settings
    [Display(Name = "LDAP Server")]
    public string? LdapServer { get; set; }

    [Display(Name = "Port")]
    public int LdapPort { get; set; } = 389;

    [Display(Name = "Base DN")]
    public string? LdapBaseDN { get; set; }

    [Display(Name = "Domain")]
    public string? LdapDomain { get; set; }

    [Display(Name = "Enable SSL/TLS")]
    public bool LdapUseSsl { get; set; }

    // Service Account (for group queries)
    [Display(Name = "Bind DN")]
    public string? LdapBindDN { get; set; }

    [Display(Name = "Bind Password")]
    [DataType(DataType.Password)]
    public string? LdapBindPassword { get; set; }

    // Role Mapping
    [Display(Name = "Admin Group DN")]
    public string? LdapAdminGroup { get; set; }

    [Display(Name = "User Group DN")]
    public string? LdapUserGroup { get; set; }

    [Display(Name = "Fallback to Local Database")]
    public bool LdapFallbackToLocal { get; set; } = true;

    // Certificate Validation (SEC-001)
    [Display(Name = "Validate SSL Certificate")]
    public bool LdapValidateCertificate { get; set; } = true;

    [Display(Name = "Certificate Thumbprint")]
    public string? LdapCertificateThumbprint { get; set; }

    // Admin account info (auto-created for LocalDB only)
    [Display(Name = "Admin Email (optional)")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? AdminEmail { get; set; }
}
