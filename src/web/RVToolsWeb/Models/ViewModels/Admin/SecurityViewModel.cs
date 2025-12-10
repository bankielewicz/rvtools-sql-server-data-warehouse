namespace RVToolsWeb.Models.ViewModels.Admin;

/// <summary>
/// View model for Security tab in Settings
/// </summary>
public class SecurityViewModel
{
    public AuthSettingsViewModel AuthSettings { get; set; } = new();
    public IEnumerable<UserViewModel> Users { get; set; } = Enumerable.Empty<UserViewModel>();
}

/// <summary>
/// View model for authentication settings display
/// </summary>
public class AuthSettingsViewModel
{
    public string AuthProvider { get; set; } = "LocalDB";
    public string? LdapServer { get; set; }
    public string? LdapDomain { get; set; }
    public string? LdapBaseDN { get; set; }
    public bool IsConfigured { get; set; }
}

/// <summary>
/// View model for user list display
/// </summary>
public class UserViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
    public bool ForcePasswordChange { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
