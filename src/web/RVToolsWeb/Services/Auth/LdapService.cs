namespace RVToolsWeb.Services.Auth;

using System.DirectoryServices.Protocols;
using System.Net;
using RVToolsWeb.Data;
using Dapper;

/// <summary>
/// Service for LDAP/Active Directory authentication using System.DirectoryServices.Protocols
/// for cross-platform support (Windows, Linux, macOS)
/// </summary>
public class LdapService : ILdapService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<LdapService> _logger;

    public LdapService(ISqlConnectionFactory connectionFactory, ILogger<LdapService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<LdapAuthResult> AuthenticateAsync(string username, string password)
    {
        var result = new LdapAuthResult { Username = username };

        try
        {
            // Get LDAP settings from database
            var settings = await GetLdapSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.LdapServer))
            {
                result.ErrorMessage = "LDAP is not configured";
                return result;
            }

            // Build the user DN for binding
            // For AD, we can bind with domain\username or username@domain format
            var bindUsername = BuildBindUsername(username, settings.LdapDomain);

            // Attempt to bind with user credentials
            using var connection = CreateConnection(settings.LdapServer, settings.LdapPort, settings.LdapUseSsl);
            var credential = new NetworkCredential(bindUsername, password);
            connection.Credential = credential;
            connection.AuthType = AuthType.Basic;

            try
            {
                connection.Bind();
                _logger.LogInformation("LDAP authentication successful for user: {Username}", username);
            }
            catch (LdapException ex)
            {
                _logger.LogWarning("LDAP authentication failed for user {Username}: {Error}", username, ex.Message);
                result.ErrorMessage = "Invalid username or password";
                return result;
            }

            // Get user attributes and group memberships
            var userInfo = await GetUserInfoAsync(connection, username, settings.LdapBaseDN);
            if (userInfo != null)
            {
                result.Email = userInfo.Email;
                result.DisplayName = userInfo.DisplayName;
                result.Groups = userInfo.Groups;
            }

            // Determine role based on group membership
            result.Role = DetermineRole(result.Groups, settings.LdapAdminGroup, settings.LdapUserGroup);
            result.Success = true;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP authentication error for user: {Username}", username);
            result.ErrorMessage = $"LDAP error: {ex.Message}";
            return result;
        }
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync(
        string server,
        int port,
        bool useSsl,
        string baseDn,
        string? bindDn = null,
        string? bindPassword = null)
    {
        try
        {
            using var connection = CreateConnection(server, port, useSsl);

            // If bind credentials provided, use them; otherwise attempt anonymous bind
            if (!string.IsNullOrEmpty(bindDn) && !string.IsNullOrEmpty(bindPassword))
            {
                connection.Credential = new NetworkCredential(bindDn, bindPassword);
                connection.AuthType = AuthType.Basic;
            }
            else
            {
                connection.AuthType = AuthType.Anonymous;
            }

            // Try to bind
            connection.Bind();

            // Try a simple search to verify base DN is valid
            var searchRequest = new SearchRequest(
                baseDn,
                "(objectClass=*)",
                SearchScope.Base,
                "distinguishedName"
            );

            var response = (SearchResponse)connection.SendRequest(searchRequest);

            if (response.Entries.Count > 0)
            {
                return await Task.FromResult((true, $"Successfully connected to {server}:{port}. Base DN is valid."));
            }

            return await Task.FromResult((true, $"Connected to {server}:{port}, but base DN returned no results."));
        }
        catch (LdapException ex)
        {
            _logger.LogWarning("LDAP connection test failed: {Error}", ex.Message);
            return await Task.FromResult((false, $"Connection failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP connection test error");
            return await Task.FromResult((false, $"Error: {ex.Message}"));
        }
    }

    public async Task<IEnumerable<string>> GetUserGroupsAsync(string username)
    {
        try
        {
            var settings = await GetLdapSettingsAsync();
            if (settings == null || string.IsNullOrEmpty(settings.LdapServer))
            {
                return Enumerable.Empty<string>();
            }

            // Need service account credentials to query groups
            if (string.IsNullOrEmpty(settings.LdapBindDN) || string.IsNullOrEmpty(settings.LdapBindPassword))
            {
                _logger.LogWarning("Cannot query groups without service account credentials");
                return Enumerable.Empty<string>();
            }

            using var connection = CreateConnection(settings.LdapServer, settings.LdapPort, settings.LdapUseSsl);
            connection.Credential = new NetworkCredential(settings.LdapBindDN, settings.LdapBindPassword);
            connection.AuthType = AuthType.Basic;
            connection.Bind();

            var userInfo = await GetUserInfoAsync(connection, username, settings.LdapBaseDN);
            return userInfo?.Groups ?? Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for user: {Username}", username);
            return Enumerable.Empty<string>();
        }
    }

    private LdapConnection CreateConnection(string server, int port, bool useSsl)
    {
        var identifier = new LdapDirectoryIdentifier(server, port);
        var connection = new LdapConnection(identifier)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Set session options
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

        if (useSsl)
        {
            connection.SessionOptions.SecureSocketLayer = true;
            // In production, you should validate certificates properly
            // For now, we'll skip validation for self-signed certs (common in enterprise AD)
            connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
        }

        return connection;
    }

    private string BuildBindUsername(string username, string? domain)
    {
        // If username already contains domain prefix or @ suffix, use as-is
        if (username.Contains('\\') || username.Contains('@'))
        {
            return username;
        }

        // If domain is provided, use UPN format (user@domain)
        if (!string.IsNullOrEmpty(domain))
        {
            return $"{username}@{domain}";
        }

        return username;
    }

    private async Task<UserInfo?> GetUserInfoAsync(LdapConnection connection, string username, string? baseDn)
    {
        if (string.IsNullOrEmpty(baseDn))
        {
            return null;
        }

        try
        {
            // Search for user by sAMAccountName (AD) or uid (generic LDAP)
            var searchFilter = $"(|(sAMAccountName={EscapeLdapFilter(username)})(uid={EscapeLdapFilter(username)}))";
            var searchRequest = new SearchRequest(
                baseDn,
                searchFilter,
                SearchScope.Subtree,
                "mail", "displayName", "memberOf", "distinguishedName"
            );

            var response = (SearchResponse)connection.SendRequest(searchRequest);

            if (response.Entries.Count == 0)
            {
                return null;
            }

            var entry = response.Entries[0];
            var userInfo = new UserInfo
            {
                Email = GetAttributeValue(entry, "mail"),
                DisplayName = GetAttributeValue(entry, "displayName"),
                Groups = GetAttributeValues(entry, "memberOf")
            };

            return await Task.FromResult(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error getting user info: {Error}", ex.Message);
            return null;
        }
    }

    private string DetermineRole(IEnumerable<string> groups, string? adminGroup, string? userGroup)
    {
        var groupList = groups.ToList();

        // Check if user is in admin group
        if (!string.IsNullOrEmpty(adminGroup))
        {
            if (groupList.Any(g => g.Equals(adminGroup, StringComparison.OrdinalIgnoreCase) ||
                                   g.Contains(adminGroup, StringComparison.OrdinalIgnoreCase)))
            {
                return "Admin";
            }
        }

        // Check if user is in user group (or if no user group specified, default to User)
        if (string.IsNullOrEmpty(userGroup))
        {
            return "User";
        }

        if (groupList.Any(g => g.Equals(userGroup, StringComparison.OrdinalIgnoreCase) ||
                               g.Contains(userGroup, StringComparison.OrdinalIgnoreCase)))
        {
            return "User";
        }

        // If user group is specified but user is not a member, deny access
        return "None";
    }

    private string? GetAttributeValue(SearchResultEntry entry, string attributeName)
    {
        if (entry.Attributes.Contains(attributeName) && entry.Attributes[attributeName].Count > 0)
        {
            return entry.Attributes[attributeName][0]?.ToString();
        }
        return null;
    }

    private IEnumerable<string> GetAttributeValues(SearchResultEntry entry, string attributeName)
    {
        var values = new List<string>();
        if (entry.Attributes.Contains(attributeName))
        {
            foreach (var value in entry.Attributes[attributeName])
            {
                if (value != null)
                {
                    values.Add(value.ToString()!);
                }
            }
        }
        return values;
    }

    private string EscapeLdapFilter(string input)
    {
        // Escape special LDAP filter characters
        return input
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }

    private async Task<LdapSettings?> GetLdapSettingsAsync()
    {
        const string sql = @"
            SELECT
                LdapServer,
                LdapDomain,
                LdapBaseDN,
                LdapPort,
                LdapUseSsl,
                LdapBindDN,
                LdapBindPassword,
                LdapAdminGroup,
                LdapUserGroup,
                LdapFallbackToLocal
            FROM Web.AuthSettings";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<LdapSettings>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get LDAP settings");
            return null;
        }
    }

    private class UserInfo
    {
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public IEnumerable<string> Groups { get; set; } = Enumerable.Empty<string>();
    }

    private class LdapSettings
    {
        public string? LdapServer { get; set; }
        public string? LdapDomain { get; set; }
        public string? LdapBaseDN { get; set; }
        public int LdapPort { get; set; } = 389;
        public bool LdapUseSsl { get; set; }
        public string? LdapBindDN { get; set; }
        public string? LdapBindPassword { get; set; }
        public string? LdapAdminGroup { get; set; }
        public string? LdapUserGroup { get; set; }
        public bool LdapFallbackToLocal { get; set; } = true;
    }
}
