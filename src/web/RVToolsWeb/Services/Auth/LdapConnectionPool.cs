namespace RVToolsWeb.Services.Auth;

using System.Collections.Concurrent;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data;

/// <summary>
/// LDAP connection pool for service account connections.
/// Used for user info lookups (group membership) - NOT for user authentication.
/// User authentication always uses a fresh connection with user credentials.
/// </summary>
public class LdapConnectionPool : ILdapConnectionPool, IDisposable
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICredentialProtectionService _credentialProtection;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LdapConnectionPool> _logger;
    private readonly AuthenticationConfig _authConfig;

    private readonly ConcurrentBag<LdapConnection> _pool = new();
    private readonly SemaphoreSlim _poolSemaphore;

    private volatile bool _isDisposed;

    private const string LdapSettingsCacheKey = "LdapSettings_Pool";

    public LdapConnectionPool(
        ISqlConnectionFactory connectionFactory,
        ICredentialProtectionService credentialProtection,
        IMemoryCache cache,
        IOptions<AppSettings> settings,
        ILogger<LdapConnectionPool> logger)
    {
        _connectionFactory = connectionFactory;
        _credentialProtection = credentialProtection;
        _cache = cache;
        _logger = logger;
        _authConfig = settings.Value.Authentication;
        _poolSemaphore = new SemaphoreSlim(_authConfig.LdapConnectionPoolSize, _authConfig.LdapConnectionPoolSize);
    }

    public bool IsAvailable
    {
        get
        {
            var settings = GetCachedSettings();
            return settings != null &&
                   !string.IsNullOrEmpty(settings.LdapServer) &&
                   !string.IsNullOrEmpty(settings.LdapBindDN) &&
                   !string.IsNullOrEmpty(settings.LdapBindPassword);
        }
    }

    public async Task<LdapConnection?> GetConnectionAsync()
    {
        if (_isDisposed) return null;

        var settings = GetCachedSettings();
        if (settings == null || string.IsNullOrEmpty(settings.LdapBindDN))
        {
            _logger.LogDebug("LDAP connection pool not available - no service account configured");
            return null;
        }

        // Wait for a slot in the pool
        if (!await _poolSemaphore.WaitAsync(TimeSpan.FromSeconds(5)))
        {
            _logger.LogWarning("LDAP connection pool timeout - all connections in use");
            return null;
        }

        try
        {
            // Try to get an existing connection from the pool
            if (_pool.TryTake(out var existingConnection))
            {
                // Validate the connection is still usable
                if (IsConnectionValid(existingConnection))
                {
                    _logger.LogDebug("Returning pooled LDAP connection");
                    return existingConnection;
                }

                // Connection is stale, dispose it
                _logger.LogDebug("Disposing stale LDAP connection");
                try { existingConnection.Dispose(); } catch { /* ignore */ }
            }

            // Create a new connection
            var connection = await CreateServiceAccountConnectionAsync(settings);
            if (connection != null)
            {
                _logger.LogDebug("Created new pooled LDAP connection");
            }
            return connection;
        }
        catch (Exception ex)
        {
            _poolSemaphore.Release();
            _logger.LogError(ex, "Error getting LDAP connection from pool");
            return null;
        }
    }

    public void ReturnConnection(LdapConnection connection)
    {
        if (_isDisposed)
        {
            try { connection.Dispose(); } catch { /* ignore */ }
            return;
        }

        // Return to pool for reuse
        _pool.Add(connection);
        _poolSemaphore.Release();
        _logger.LogDebug("Connection returned to LDAP pool");
    }

    public void InvalidatePool()
    {
        _logger.LogInformation("Invalidating LDAP connection pool");

        // Clear cached settings
        _cache.Remove(LdapSettingsCacheKey);

        // Dispose all pooled connections
        while (_pool.TryTake(out var connection))
        {
            try { connection.Dispose(); } catch { /* ignore */ }
        }
    }

    private LdapSettings? GetCachedSettings()
    {
        return _cache.GetOrCreate(LdapSettingsCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_authConfig.LdapSettingsCacheMinutes);
            return LoadLdapSettingsSync();
        });
    }

    private LdapSettings? LoadLdapSettingsSync()
    {
        const string sql = @"
            SELECT
                LdapServer, LdapDomain, LdapBaseDN, LdapPort, LdapUseSsl,
                LdapBindDN, LdapBindPassword, LdapAdminGroup, LdapUserGroup,
                LdapFallbackToLocal, LdapValidateCertificate, LdapCertificateThumbprint
            FROM Web.AuthSettings";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var settings = connection.QuerySingleOrDefault<LdapSettings>(sql);

            if (settings != null && !string.IsNullOrEmpty(settings.LdapBindPassword))
            {
                settings.LdapBindPassword = _credentialProtection.Decrypt(settings.LdapBindPassword);
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load LDAP settings for connection pool");
            return null;
        }
    }

    private async Task<LdapConnection?> CreateServiceAccountConnectionAsync(LdapSettings settings)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier(settings.LdapServer!, settings.LdapPort);
            var connection = new LdapConnection(identifier)
            {
                Timeout = TimeSpan.FromSeconds(_authConfig.LdapConnectionTimeoutSeconds)
            };

            connection.SessionOptions.ProtocolVersion = 3;
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            if (settings.LdapUseSsl)
            {
                connection.SessionOptions.SecureSocketLayer = true;

                if (settings.LdapValidateCertificate)
                {
                    if (!string.IsNullOrEmpty(settings.LdapCertificateThumbprint))
                    {
                        connection.SessionOptions.VerifyServerCertificate = (conn, cert) =>
                            ValidateCertificateByThumbprint(cert, settings.LdapCertificateThumbprint, settings.LdapServer!);
                    }
                }
                else
                {
                    _logger.LogWarning("LDAP certificate validation disabled for connection pool");
                    connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                }
            }

            // Bind with service account credentials
            connection.Credential = new NetworkCredential(settings.LdapBindDN, settings.LdapBindPassword);
            connection.AuthType = AuthType.Basic;

            // Perform bind asynchronously
            await Task.Run(() => connection.Bind());

            _logger.LogDebug("Service account LDAP connection established to {Server}", settings.LdapServer);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service account LDAP connection");
            return null;
        }
    }

    private bool IsConnectionValid(LdapConnection connection)
    {
        try
        {
            // Send a simple search to test connection
            var settings = GetCachedSettings();
            if (settings?.LdapBaseDN == null) return false;

            var searchRequest = new SearchRequest(
                settings.LdapBaseDN,
                "(objectClass=*)",
                SearchScope.Base,
                "distinguishedName"
            );
            searchRequest.TimeLimit = TimeSpan.FromSeconds(2);

            var response = (SearchResponse)connection.SendRequest(searchRequest);
            return response.Entries.Count > 0;
        }
        catch
        {
            return false;
        }
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
                return true;
            }

            _logger.LogError(
                "LDAP pool certificate thumbprint mismatch for {Server}. Expected: {Expected}, Actual: {Actual}",
                server, normalizedExpected, normalizedActual);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating LDAP pool certificate for {Server}", server);
            return false;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Dispose all pooled connections
        while (_pool.TryTake(out var connection))
        {
            try { connection.Dispose(); } catch { /* ignore */ }
        }

        _poolSemaphore.Dispose();
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
