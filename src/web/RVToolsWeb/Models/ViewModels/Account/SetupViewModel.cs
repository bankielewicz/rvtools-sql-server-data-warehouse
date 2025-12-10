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

    // LDAP settings (for future use - disabled in MVP)
    [Display(Name = "LDAP Server")]
    public string? LdapServer { get; set; }

    [Display(Name = "LDAP Domain")]
    public string? LdapDomain { get; set; }

    [Display(Name = "LDAP Base DN")]
    public string? LdapBaseDN { get; set; }

    // Admin account info (auto-created with admin/admin)
    [Display(Name = "Admin Email (optional)")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? AdminEmail { get; set; }
}
