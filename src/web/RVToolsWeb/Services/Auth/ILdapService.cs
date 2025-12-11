namespace RVToolsWeb.Services.Auth;

/// <summary>
/// Result of LDAP authentication attempt
/// </summary>
public class LdapAuthResult
{
    public bool Success { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string Role { get; set; } = "User";
    public IEnumerable<string> Groups { get; set; } = Enumerable.Empty<string>();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Service for LDAP/Active Directory authentication
/// </summary>
public interface ILdapService
{
    /// <summary>
    /// Authenticate user against LDAP/AD server
    /// </summary>
    /// <param name="username">Username (sAMAccountName for AD)</param>
    /// <param name="password">User's password</param>
    /// <returns>Authentication result with user info and role</returns>
    Task<LdapAuthResult> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Test LDAP connection with provided settings
    /// </summary>
    /// <param name="server">LDAP server hostname or IP</param>
    /// <param name="port">LDAP port (389 or 636)</param>
    /// <param name="useSsl">Use SSL/TLS</param>
    /// <param name="baseDn">Base DN for searches</param>
    /// <param name="bindDn">Service account DN (optional)</param>
    /// <param name="bindPassword">Service account password (optional)</param>
    /// <returns>True if connection successful, false otherwise</returns>
    Task<(bool Success, string Message)> TestConnectionAsync(
        string server,
        int port,
        bool useSsl,
        string baseDn,
        string? bindDn = null,
        string? bindPassword = null);

    /// <summary>
    /// Get user's group memberships from LDAP
    /// </summary>
    /// <param name="username">Username to query</param>
    /// <returns>List of group DNs the user is a member of</returns>
    Task<IEnumerable<string>> GetUserGroupsAsync(string username);
}
