namespace RVToolsWeb.Services.Auth;

using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data;

/// <summary>
/// Service for LDAP/Active Directory authentication using System.DirectoryServices.Protocols
/// for cross-platform support (Windows, Linux, macOS).
/// Optimized with settings caching and connection pooling for improved performance.
/// </summary>
public class LdapService : ILdapService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICredentialProtectionService _credentialProtection;
    private readonly IMemoryCache _cache;
    private readonly ILdapConnectionPool _connectionPool;
    private readonly ILogger<LdapService> _logger;
    private readonly AuthenticationConfig _authConfig;

    private const string LdapSettingsCacheKey = "LdapSettings";

    public LdapService(
        ISqlConnectionFactory connectionFactory,
        ICredentialProtectionService credentialProtection,
        IMemoryCache cache,
        ILdapConnectionPool connectionPool,
        IOptions<AppSettings> settings,
        ILogger<LdapService> logger)
    {
        _connectionFactory = connectionFactory;
        _credentialProtection = credentialProtection;
        _cache = cache;
        _connectionPool = connectionPool;
        _logger = logger;
        _authConfig = settings.Value.Authentication;
    }

    public async Task<LdapAuthResult> AuthenticateAsync(string username, string password)
    {
        var result = new LdapAuthResult { Username = username };
        var totalStopwatch = Stopwatch.StartNew();
        var stepStopwatch = new Stopwatch();

        try
        {
            // Get LDAP settings from cache or database
            stepStopwatch.Start();
            var settings = await GetLdapSettingsAsync();
            LogTiming("Settings retrieval", stepStopwatch);

            if (settings == null || string.IsNullOrEmpty(settings.LdapServer))
            {
                result.ErrorMessage = "LDAP is not configured";
                return result;
            }

            // Build the user DN for binding
            var bindUsername = BuildBindUsername(username, settings.LdapDomain);

            // Attempt to bind with user credentials (NEVER pooled - must verify credentials)
            stepStopwatch.Restart();
            using var connection = CreateConnection(
                settings.LdapServer,
                settings.LdapPort,
                settings.LdapUseSsl,
                settings.LdapValidateCertificate,
                settings.LdapCertificateThumbprint);
            LogTiming("Connection creation", stepStopwatch);

            var credential = new NetworkCredential(bindUsername, password);
            connection.Credential = credential;
            connection.AuthType = AuthType.Basic;

            try
            {
                stepStopwatch.Restart();
                connection.Bind();
                LogTiming("User bind (authentication)", stepStopwatch);
                _logger.LogInformation("LDAP authentication successful for user: {Username}", username);
            }
            catch (LdapException ex)
            {
                _logger.LogWarning("LDAP authentication failed for user {Username}: {Error}", username, ex.Message);
                result.ErrorMessage = "Invalid username or password";
                return result;
            }

            // Get user attributes and group memberships using pooled connection if available
            stepStopwatch.Restart();
            var userInfo = await GetUserInfoWithPoolAsync(username, settings);
            LogTiming("User info retrieval", stepStopwatch);

            if (userInfo != null)
            {
                result.Email = userInfo.Email;
                result.DisplayName = userInfo.DisplayName;
                result.Groups = userInfo.Groups;
            }

            // Determine role based on group membership
            result.Role = DetermineRole(result.Groups, settings.LdapAdminGroup, settings.LdapUserGroup);
            result.Success = true;

            totalStopwatch.Stop();
            _logger.LogInformation(
                "LDAP authentication completed for {Username} in {TotalMs}ms (Role: {Role})",
                username, totalStopwatch.ElapsedMilliseconds, result.Role);

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

            if (!string.IsNullOrEmpty(bindDn) && !string.IsNullOrEmpty(bindPassword))
            {
                connection.Credential = new NetworkCredential(bindDn, bindPassword);
                connection.AuthType = AuthType.Basic;
            }
            else
            {
                connection.AuthType = AuthType.Anonymous;
            }

            connection.Bind();

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

            if (string.IsNullOrEmpty(settings.LdapBindDN) || string.IsNullOrEmpty(settings.LdapBindPassword))
            {
                _logger.LogWarning("Cannot query groups without service account credentials");
                return Enumerable.Empty<string>();
            }

            // Try to use pooled connection
            var pooledConnection = await _connectionPool.GetConnectionAsync();
            if (pooledConnection != null)
            {
                try
                {
                    var userInfo = await GetUserInfoAsync(pooledConnection, username, settings.LdapBaseDN);
                    return userInfo?.Groups ?? Enumerable.Empty<string>();
                }
                finally
                {
                    _connectionPool.ReturnConnection(pooledConnection);
                }
            }

            // Fallback to creating a new connection
            using var connection = CreateConnection(
                settings.LdapServer,
                settings.LdapPort,
                settings.LdapUseSsl,
                settings.LdapValidateCertificate,
                settings.LdapCertificateThumbprint);
            connection.Credential = new NetworkCredential(settings.LdapBindDN, settings.LdapBindPassword);
            connection.AuthType = AuthType.Basic;
            connection.Bind();

            var info = await GetUserInfoAsync(connection, username, settings.LdapBaseDN);
            return info?.Groups ?? Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for user: {Username}", username);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Invalidates the cached LDAP settings. Call when settings are updated.
    /// </summary>
    public void InvalidateSettingsCache()
    {
        _cache.Remove(LdapSettingsCacheKey);
        _connectionPool.InvalidatePool();
        _logger.LogInformation("LDAP settings cache invalidated");
    }

    private async Task<UserInfo?> GetUserInfoWithPoolAsync(string username, LdapSettings settings)
    {
        // Try pooled connection first (service account)
        if (_connectionPool.IsAvailable)
        {
            var pooledConnection = await _connectionPool.GetConnectionAsync();
            if (pooledConnection != null)
            {
                try
                {
                    _logger.LogDebug("Using pooled connection for user info lookup");
                    return await GetUserInfoAsync(pooledConnection, username, settings.LdapBaseDN);
                }
                finally
                {
                    _connectionPool.ReturnConnection(pooledConnection);
                }
            }
        }

        // Fallback: use service account with new connection
        if (!string.IsNullOrEmpty(settings.LdapBindDN) && !string.IsNullOrEmpty(settings.LdapBindPassword))
        {
            _logger.LogDebug("Using new service account connection for user info lookup");
            using var serviceConnection = CreateConnection(
                settings.LdapServer!,
                settings.LdapPort,
                settings.LdapUseSsl,
                settings.LdapValidateCertificate,
                settings.LdapCertificateThumbprint);
            serviceConnection.Credential = new NetworkCredential(settings.LdapBindDN, settings.LdapBindPassword);
            serviceConnection.AuthType = AuthType.Basic;
            serviceConnection.Bind();

            return await GetUserInfoAsync(serviceConnection, username, settings.LdapBaseDN);
        }

        _logger.LogWarning("No service account available for user info lookup");
        return null;
    }

    private LdapConnection CreateConnection(string server, int port, bool useSsl,
        bool validateCertificate = true, string? certificateThumbprint = null)
    {
        var identifier = new LdapDirectoryIdentifier(server, port);
        var connection = new LdapConnection(identifier)
        {
            Timeout = TimeSpan.FromSeconds(_authConfig.LdapConnectionTimeoutSeconds)
        };

        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

        if (useSsl)
        {
            connection.SessionOptions.SecureSocketLayer = true;

            if (validateCertificate)
            {
                if (!string.IsNullOrEmpty(certificateThumbprint))
                {
                    connection.SessionOptions.VerifyServerCertificate = (conn, cert) =>
                        ValidateCertificateByThumbprint(cert, certificateThumbprint, server);
                }
            }
            else
            {
                _logger.LogWarning(
                    "LDAP certificate validation is DISABLED for server {Server}. " +
                    "This is insecure and should only be used for testing.",
                    server);
                connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
            }
        }

        return connection;
    }

    private bool ValidateCertificateByThumbprint(X509Certificate certificate, string expectedThumbprint, string server)
    {
        try
        {
            using var x509Cert = new X509Certificate2(certificate);
            var actualThumbprint = x509Cert.Thumbprint;

            var normalizedExpected = expectedThumbprint
                .Replace(" ", "")
                .Replace(":", "")
                .Replace("-", "")
                .ToUpperInvariant();
            var normalizedActual = actualThumbprint.ToUpperInvariant();

            if (normalizedActual == normalizedExpected)
            {
                _logger.LogDebug("LDAP certificate thumbprint matched for server {Server}", server);
                return true;
            }

            _logger.LogError(
                "LDAP certificate thumbprint mismatch for server {Server}. " +
                "Expected: {Expected}, Actual: {Actual}",
                server, normalizedExpected, normalizedActual);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating LDAP certificate for server {Server}", server);
            return false;
        }
    }

    private string BuildBindUsername(string username, string? domain)
    {
        if (username.Contains('\\') || username.Contains('@'))
        {
            return username;
        }

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
            var searchUsername = username;
            if (username.Contains('\\'))
            {
                searchUsername = username.Split('\\')[1];
            }
            else if (username.Contains('@'))
            {
                searchUsername = username.Split('@')[0];
            }

            var searchFilter = $"(|(sAMAccountName={EscapeLdapFilter(searchUsername)})(uid={EscapeLdapFilter(searchUsername)}))";
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

        _logger.LogDebug("DetermineRole called with {GroupCount} groups", groupList.Count);
        if (_authConfig.LdapDetailedTimingLogs)
        {
            _logger.LogDebug("User's groups: {Groups}", string.Join(" | ", groupList));
            _logger.LogDebug("Configured AdminGroup: {AdminGroup}", adminGroup ?? "(null)");
            _logger.LogDebug("Configured UserGroup: {UserGroup}", userGroup ?? "(null)");
        }

        if (!string.IsNullOrEmpty(adminGroup))
        {
            var matchedAdmin = groupList.FirstOrDefault(g =>
                g.Equals(adminGroup, StringComparison.OrdinalIgnoreCase) ||
                g.Contains(adminGroup, StringComparison.OrdinalIgnoreCase));

            if (matchedAdmin != null)
            {
                _logger.LogDebug("User matched Admin group: {MatchedGroup}", matchedAdmin);
                return "Admin";
            }
        }

        if (string.IsNullOrEmpty(userGroup))
        {
            return "User";
        }

        if (groupList.Any(g => g.Equals(userGroup, StringComparison.OrdinalIgnoreCase) ||
                               g.Contains(userGroup, StringComparison.OrdinalIgnoreCase)))
        {
            return "User";
        }

        return "None";
    }

    private string? GetAttributeValue(SearchResultEntry entry, string attributeName)
    {
        if (entry.Attributes.Contains(attributeName) && entry.Attributes[attributeName].Count > 0)
        {
            var value = entry.Attributes[attributeName][0];
            if (value is byte[] byteArray)
            {
                return System.Text.Encoding.UTF8.GetString(byteArray);
            }
            return value?.ToString();
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
                    if (value is byte[] byteArray)
                    {
                        values.Add(System.Text.Encoding.UTF8.GetString(byteArray));
                    }
                    else
                    {
                        values.Add(value.ToString()!);
                    }
                }
            }
        }
        return values;
    }

    private string EscapeLdapFilter(string input)
    {
        return input
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }

    private async Task<LdapSettings?> GetLdapSettingsAsync()
    {
        return await _cache.GetOrCreateAsync(LdapSettingsCacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_authConfig.LdapSettingsCacheMinutes);
            return await LoadLdapSettingsFromDbAsync();
        });
    }

    private async Task<LdapSettings?> LoadLdapSettingsFromDbAsync()
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
                LdapFallbackToLocal,
                LdapValidateCertificate,
                LdapCertificateThumbprint
            FROM Web.AuthSettings";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var settings = await connection.QuerySingleOrDefaultAsync<LdapSettings>(sql);

            if (settings != null && !string.IsNullOrEmpty(settings.LdapBindPassword))
            {
                settings.LdapBindPassword = _credentialProtection.Decrypt(settings.LdapBindPassword);
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get LDAP settings");
            return null;
        }
    }

    private void LogTiming(string operation, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        if (_authConfig.LdapDetailedTimingLogs)
        {
            _logger.LogDebug("LDAP timing - {Operation}: {ElapsedMs}ms", operation, stopwatch.ElapsedMilliseconds);
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
        public bool LdapValidateCertificate { get; set; } = true;
        public string? LdapCertificateThumbprint { get; set; }
    }
}
