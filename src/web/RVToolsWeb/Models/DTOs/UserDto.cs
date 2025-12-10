namespace RVToolsWeb.Models.DTOs;

/// <summary>
/// Data transfer object for Web.Users table
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Authentication source: "LocalDB" or "LDAP" (not persisted, set at runtime)
    /// </summary>
    public string AuthSource { get; set; } = "LocalDB";
}

/// <summary>
/// Data transfer object for Web.AuthSettings table
/// </summary>
public class AuthSettingsDto
{
    public int AuthSettingsId { get; set; }
    public string AuthProvider { get; set; } = "LocalDB";
    public string? LdapServer { get; set; }
    public string? LdapDomain { get; set; }
    public string? LdapBaseDN { get; set; }
    public string? LdapSearchFilter { get; set; }
    public int LdapPort { get; set; } = 389;
    public bool LdapUseSsl { get; set; }
    public string? LdapBindDN { get; set; }
    public string? LdapBindPassword { get; set; }
    public string? LdapAdminGroup { get; set; }
    public string? LdapUserGroup { get; set; }
    public bool LdapFallbackToLocal { get; set; } = true;
    public bool IsConfigured { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
