namespace RVToolsWeb.Services.Auth;

using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for user account management
/// </summary>
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<int> GetUserCountAsync();

    /// <summary>
    /// Validate credentials and return authentication result.
    /// Uses configured authentication provider (LocalDB or LDAP with fallback).
    /// LDAP users are authenticated transiently (no database record created).
    /// </summary>
    /// <returns>Tuple with: Success, User (transient for LDAP), AuthSource, ErrorMessage</returns>
    Task<(bool Success, UserDto? User, string AuthSource, string? ErrorMessage)> ValidateCredentialsAsync(
        string username, string password);

    /// <summary>
    /// Create a new user account
    /// </summary>
    Task<bool> CreateUserAsync(string username, string password, string role,
        string? email = null, bool forcePasswordChange = false);

    /// <summary>
    /// Update user's password
    /// </summary>
    Task<bool> UpdatePasswordAsync(int userId, string newPassword);

    /// <summary>
    /// Update user's password with force change option (admin reset)
    /// </summary>
    Task<bool> ResetPasswordAsync(int userId, string newPassword, bool forceChange = true);

    /// <summary>
    /// Update user profile (email, role, active status)
    /// </summary>
    Task<bool> UpdateUserAsync(int userId, string? email, string role, bool isActive);

    /// <summary>
    /// Delete a user account (cannot delete 'admin')
    /// </summary>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>
    /// Record login attempt (success or failure) for lockout tracking
    /// </summary>
    Task RecordLoginAttemptAsync(int userId, bool success);

    /// <summary>
    /// Reset failed login attempts counter
    /// </summary>
    Task ResetFailedAttemptsAsync(int userId);
}
