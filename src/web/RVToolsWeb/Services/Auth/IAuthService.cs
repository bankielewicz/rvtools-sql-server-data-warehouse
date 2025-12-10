namespace RVToolsWeb.Services.Auth;

using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for authentication settings management
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Get current authentication settings
    /// </summary>
    Task<AuthSettingsDto?> GetAuthSettingsAsync();

    /// <summary>
    /// Check if first-time setup is required (IsConfigured = false)
    /// </summary>
    Task<bool> IsSetupRequiredAsync();

    /// <summary>
    /// Complete first-time setup and mark as configured
    /// </summary>
    Task<bool> CompleteSetupAsync(string authProvider, string? ldapServer = null,
        string? ldapDomain = null, string? ldapBaseDN = null);

    /// <summary>
    /// Update authentication provider settings (Admin only)
    /// </summary>
    Task<bool> UpdateAuthSettingsAsync(string authProvider, string? ldapServer = null,
        string? ldapDomain = null, string? ldapBaseDN = null);

    /// <summary>
    /// Update full LDAP settings including port, SSL, bind credentials, and group mappings
    /// </summary>
    Task<bool> UpdateLdapSettingsAsync(
        string ldapServer,
        string? ldapDomain,
        string ldapBaseDN,
        int ldapPort,
        bool ldapUseSsl,
        string? ldapBindDN,
        string? ldapBindPassword,
        string? ldapAdminGroup,
        string? ldapUserGroup,
        bool ldapFallbackToLocal);
}
