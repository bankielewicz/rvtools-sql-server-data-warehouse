namespace RVToolsWeb.Services.Auth;

using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for authentication settings management
/// </summary>
public class AuthService : IAuthService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ISqlConnectionFactory connectionFactory, ILogger<AuthService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<AuthSettingsDto?> GetAuthSettingsAsync()
    {
        const string sql = "SELECT TOP 1 * FROM Web.AuthSettings";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<AuthSettingsDto>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get auth settings");
            return null;
        }
    }

    public async Task<bool> IsSetupRequiredAsync()
    {
        const string sql = "SELECT TOP 1 IsConfigured FROM Web.AuthSettings";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var isConfigured = await connection.QuerySingleOrDefaultAsync<bool?>(sql);
            return isConfigured != true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check setup status");
            return true; // Assume setup required if check fails
        }
    }

    public async Task<bool> CompleteSetupAsync(string authProvider, string? ldapServer = null,
        string? ldapDomain = null, string? ldapBaseDN = null)
    {
        const string sql = @"
            UPDATE Web.AuthSettings
            SET AuthProvider = @AuthProvider,
                LdapServer = @LdapServer,
                LdapDomain = @LdapDomain,
                LdapBaseDN = @LdapBaseDN,
                IsConfigured = 1,
                ModifiedDate = GETUTCDATE()";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                AuthProvider = authProvider,
                LdapServer = ldapServer,
                LdapDomain = ldapDomain,
                LdapBaseDN = ldapBaseDN
            });

            _logger.LogInformation("First-time setup completed with provider: {Provider}", authProvider);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete setup");
            return false;
        }
    }

    public async Task<bool> UpdateAuthSettingsAsync(string authProvider, string? ldapServer = null,
        string? ldapDomain = null, string? ldapBaseDN = null)
    {
        const string sql = @"
            UPDATE Web.AuthSettings
            SET AuthProvider = @AuthProvider,
                LdapServer = @LdapServer,
                LdapDomain = @LdapDomain,
                LdapBaseDN = @LdapBaseDN,
                ModifiedDate = GETUTCDATE()";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                AuthProvider = authProvider,
                LdapServer = ldapServer,
                LdapDomain = ldapDomain,
                LdapBaseDN = ldapBaseDN
            });

            _logger.LogInformation("Auth settings updated to provider: {Provider}", authProvider);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update auth settings");
            return false;
        }
    }

    public async Task<bool> UpdateLdapSettingsAsync(
        string ldapServer,
        string? ldapDomain,
        string ldapBaseDN,
        int ldapPort,
        bool ldapUseSsl,
        string? ldapBindDN,
        string? ldapBindPassword,
        string? ldapAdminGroup,
        string? ldapUserGroup,
        bool ldapFallbackToLocal,
        bool ldapValidateCertificate = true,
        string? ldapCertificateThumbprint = null)
    {
        const string sql = @"
            UPDATE Web.AuthSettings
            SET AuthProvider = 'LDAP',
                LdapServer = @LdapServer,
                LdapDomain = @LdapDomain,
                LdapBaseDN = @LdapBaseDN,
                LdapPort = @LdapPort,
                LdapUseSsl = @LdapUseSsl,
                LdapBindDN = @LdapBindDN,
                LdapBindPassword = @LdapBindPassword,
                LdapAdminGroup = @LdapAdminGroup,
                LdapUserGroup = @LdapUserGroup,
                LdapFallbackToLocal = @LdapFallbackToLocal,
                LdapValidateCertificate = @LdapValidateCertificate,
                LdapCertificateThumbprint = @LdapCertificateThumbprint,
                ModifiedDate = GETUTCDATE()";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                LdapServer = ldapServer,
                LdapDomain = ldapDomain,
                LdapBaseDN = ldapBaseDN,
                LdapPort = ldapPort,
                LdapUseSsl = ldapUseSsl,
                LdapBindDN = ldapBindDN,
                LdapBindPassword = ldapBindPassword,
                LdapAdminGroup = ldapAdminGroup,
                LdapUserGroup = ldapUserGroup,
                LdapFallbackToLocal = ldapFallbackToLocal,
                LdapValidateCertificate = ldapValidateCertificate,
                LdapCertificateThumbprint = ldapCertificateThumbprint
            });

            _logger.LogInformation("LDAP settings updated for server: {Server}", ldapServer);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update LDAP settings");
            return false;
        }
    }
}
