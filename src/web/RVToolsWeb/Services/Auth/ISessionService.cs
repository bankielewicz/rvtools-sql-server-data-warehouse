namespace RVToolsWeb.Services.Auth;

using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for tracking user sessions (login/logout events)
/// Used for audit trail of both LocalDB and LDAP authentications
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Record a login event for any authenticated user
    /// For LDAP users, UserId will be null in the session record
    /// </summary>
    Task RecordLoginAsync(UserDto user, string? ipAddress, string? userAgent);

    /// <summary>
    /// Record a logout event by updating the most recent session
    /// </summary>
    Task RecordLogoutAsync(string username);

    /// <summary>
    /// Get recent session records for audit/monitoring
    /// </summary>
    Task<IEnumerable<SessionDto>> GetRecentSessionsAsync(int count = 100);

    /// <summary>
    /// Get currently active sessions (logged in within last 8 hours, no logout recorded)
    /// </summary>
    Task<IEnumerable<SessionDto>> GetActiveSessionsAsync();
}
