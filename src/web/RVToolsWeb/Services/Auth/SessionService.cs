namespace RVToolsWeb.Services.Auth;

using Dapper;
using RVToolsWeb.Data;
using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for tracking user sessions (login/logout events)
/// Used for audit trail of both LocalDB and LDAP authentications
/// </summary>
public class SessionService : ISessionService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISqlConnectionFactory connectionFactory, ILogger<SessionService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task RecordLoginAsync(UserDto user, string? ipAddress, string? userAgent)
    {
        const string sql = @"
            INSERT INTO [Web].[Sessions] (Username, AuthSource, UserId, Role, Email, LoginTime, IPAddress, UserAgent)
            VALUES (@Username, @AuthSource, @UserId, @Role, @Email, GETUTCDATE(), @IPAddress, @UserAgent)";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                user.Username,
                user.AuthSource,
                UserId = user.UserId == 0 ? (int?)null : user.UserId, // NULL for LDAP users (transient)
                user.Role,
                user.Email,
                IPAddress = TruncateString(ipAddress, 50),
                UserAgent = TruncateString(userAgent, 500)
            });

            _logger.LogDebug("Recorded login session for {Username} via {AuthSource}", user.Username, user.AuthSource);
        }
        catch (Exception ex)
        {
            // Non-blocking: authentication succeeds even if audit recording fails
            _logger.LogError(ex, "Failed to record login session for {Username}", user.Username);
        }
    }

    public async Task RecordLogoutAsync(string username)
    {
        const string sql = @"
            UPDATE [Web].[Sessions]
            SET LogoutTime = GETUTCDATE()
            WHERE Username = @Username
              AND LogoutTime IS NULL
              AND LoginTime >= DATEADD(HOUR, -8, GETUTCDATE())";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rowsUpdated = await connection.ExecuteAsync(sql, new { Username = username });

            if (rowsUpdated > 0)
            {
                _logger.LogDebug("Recorded logout for {Username}", username);
            }
        }
        catch (Exception ex)
        {
            // Non-blocking: logout succeeds even if audit update fails
            _logger.LogError(ex, "Failed to record logout for {Username}", username);
        }
    }

    public async Task<IEnumerable<SessionDto>> GetRecentSessionsAsync(int count = 100)
    {
        const string sql = @"
            SELECT TOP (@Count)
                SessionId,
                Username,
                AuthSource,
                UserId,
                Role,
                Email,
                LoginTime,
                LogoutTime,
                IPAddress,
                UserAgent,
                SessionToken
            FROM [Web].[Sessions]
            ORDER BY LoginTime DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SessionDto>(sql, new { Count = count });
    }

    public async Task<IEnumerable<SessionDto>> GetActiveSessionsAsync()
    {
        const string sql = @"
            SELECT
                SessionId,
                Username,
                AuthSource,
                UserId,
                Role,
                Email,
                LoginTime,
                LogoutTime,
                IPAddress,
                UserAgent,
                SessionToken
            FROM [Web].[Sessions]
            WHERE LogoutTime IS NULL
              AND LoginTime >= DATEADD(HOUR, -8, GETUTCDATE())
            ORDER BY LoginTime DESC";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<SessionDto>(sql);
    }

    /// <summary>
    /// Truncate string to prevent SQL truncation errors
    /// </summary>
    private static string? TruncateString(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
