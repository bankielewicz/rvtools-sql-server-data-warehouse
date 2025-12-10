namespace RVToolsWeb.Services.Auth;

using System.Security.Cryptography;
using Dapper;
using Microsoft.Extensions.Options;
using RVToolsWeb.Configuration;
using RVToolsWeb.Data;
using RVToolsWeb.Models.DTOs;

/// <summary>
/// Service for user account management with PBKDF2 password hashing
/// </summary>
public class UserService : IUserService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<UserService> _logger;
    private readonly AuthenticationConfig _authConfig;
    private readonly ILdapService _ldapService;
    private readonly IAuthService _authService;

    // PBKDF2 parameters (OWASP recommended)
    private const int Iterations = 100000;
    private const int SaltSize = 32;
    private const int HashSize = 32;

    public UserService(
        ISqlConnectionFactory connectionFactory,
        ILogger<UserService> logger,
        IOptions<AppSettings> settings,
        ILdapService ldapService,
        IAuthService authService)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _authConfig = settings.Value.Authentication;
        _ldapService = ldapService;
        _authService = authService;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        const string sql = "SELECT * FROM Web.Users WHERE UserId = @UserId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { UserId = userId });
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        const string sql = "SELECT * FROM Web.Users WHERE Username = @Username";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Username = username });
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        const string sql = "SELECT * FROM Web.Users ORDER BY Username";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserDto>(sql);
    }

    public async Task<int> GetUserCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Web.Users";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string username, string password)
    {
        // Get auth settings to determine which provider to use
        var authSettings = await _authService.GetAuthSettingsAsync();
        var useLocalDb = authSettings?.AuthProvider != "LDAP";
        var fallbackToLocal = authSettings?.LdapFallbackToLocal ?? true;

        // Try LDAP first if configured
        if (!useLocalDb)
        {
            _logger.LogDebug("Attempting LDAP authentication for user: {Username}", username);
            var ldapResult = await _ldapService.AuthenticateAsync(username, password);

            if (ldapResult.Success)
            {
                _logger.LogInformation("LDAP authentication successful for user: {Username}", username);

                // Check if role is "None" (user not in required groups)
                if (ldapResult.Role == "None")
                {
                    _logger.LogWarning("LDAP user {Username} not in required AD groups", username);
                    return null;
                }

                // Get or create local user record for session tracking
                var ldapUser = await GetOrCreateLdapUserAsync(username, ldapResult.Email, ldapResult.Role);
                if (ldapUser != null)
                {
                    ldapUser.AuthSource = "LDAP";
                    await RecordLoginAttemptAsync(ldapUser.UserId, true);
                }
                return ldapUser;
            }

            // LDAP failed - try fallback to local if enabled
            if (fallbackToLocal)
            {
                _logger.LogDebug("LDAP authentication failed for {Username}, trying local auth fallback", username);
            }
            else
            {
                _logger.LogWarning("LDAP authentication failed for {Username}: {Error}", username, ldapResult.ErrorMessage);
                return null;
            }
        }

        // Local database authentication
        return await ValidateLocalCredentialsAsync(username, password);
    }

    private async Task<UserDto?> ValidateLocalCredentialsAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);

        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Username}", username);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Username}", username);
            return null;
        }

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            _logger.LogWarning("Login attempt for locked out user: {Username}", username);
            return null;
        }

        // Verify password
        var computedHash = HashPassword(password, user.Salt);
        if (computedHash != user.PasswordHash)
        {
            _logger.LogWarning("Invalid password for user: {Username}", username);
            await RecordLoginAttemptAsync(user.UserId, false);
            return null;
        }

        // Success
        user.AuthSource = "LocalDB";
        await RecordLoginAttemptAsync(user.UserId, true);
        return user;
    }

    public async Task<UserDto?> GetOrCreateLdapUserAsync(string username, string? email, string role)
    {
        var user = await GetUserByUsernameAsync(username);

        if (user != null)
        {
            // Update role if changed in AD
            if (user.Role != role || user.Email != email)
            {
                await UpdateUserAsync(user.UserId, email, role, true);
                user.Role = role;
                user.Email = email;
            }
            return user;
        }

        // Create new local record for LDAP user (no password - can't login locally)
        const string sql = @"
            INSERT INTO Web.Users (Username, PasswordHash, Salt, Email, Role, ForcePasswordChange, IsActive)
            VALUES (@Username, 'LDAP_USER_NO_PASSWORD', 'LDAP_USER_NO_SALT', @Email, @Role, 0, 1);
            SELECT * FROM Web.Users WHERE UserId = SCOPE_IDENTITY()";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var newUser = await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new
            {
                Username = username,
                Email = email,
                Role = role
            });

            _logger.LogInformation("Created local record for LDAP user {Username} with role {Role}", username, role);
            return newUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create local record for LDAP user {Username}", username);
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(string username, string password, string role,
        string? email = null, bool forcePasswordChange = false)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);

        const string sql = @"
            INSERT INTO Web.Users (Username, PasswordHash, Salt, Email, Role, ForcePasswordChange)
            VALUES (@Username, @PasswordHash, @Salt, @Email, @Role, @ForcePasswordChange)";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                Username = username,
                PasswordHash = hash,
                Salt = salt,
                Email = email,
                Role = role,
                ForcePasswordChange = forcePasswordChange
            });

            _logger.LogInformation("Created user {Username} with role {Role}", username, role);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", username);
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(newPassword, salt);

        const string sql = @"
            UPDATE Web.Users
            SET PasswordHash = @PasswordHash,
                Salt = @Salt,
                ForcePasswordChange = 0,
                ModifiedDate = GETUTCDATE()
            WHERE UserId = @UserId";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                PasswordHash = hash,
                Salt = salt
            });

            _logger.LogInformation("Password updated for user ID {UserId}", userId);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update password for user ID {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword, bool forceChange = true)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(newPassword, salt);

        const string sql = @"
            UPDATE Web.Users
            SET PasswordHash = @PasswordHash,
                Salt = @Salt,
                ForcePasswordChange = @ForceChange,
                FailedLoginAttempts = 0,
                LockoutEnd = NULL,
                ModifiedDate = GETUTCDATE()
            WHERE UserId = @UserId";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                PasswordHash = hash,
                Salt = salt,
                ForceChange = forceChange
            });

            _logger.LogInformation("Password reset for user ID {UserId}, ForceChange={ForceChange}", userId, forceChange);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password for user ID {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(int userId, string? email, string role, bool isActive)
    {
        const string sql = @"
            UPDATE Web.Users
            SET Email = @Email,
                Role = @Role,
                IsActive = @IsActive,
                ModifiedDate = GETUTCDATE()
            WHERE UserId = @UserId";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Email = email,
                Role = role,
                IsActive = isActive
            });

            _logger.LogInformation("Updated user ID {UserId}: Role={Role}, IsActive={IsActive}",
                userId, role, isActive);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user ID {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        // Prevent deleting the admin account
        var user = await GetUserByIdAsync(userId);
        if (user?.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning("Attempted to delete protected admin account");
            return false;
        }

        const string sql = "DELETE FROM Web.Users WHERE UserId = @UserId";

        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new { UserId = userId });

            _logger.LogInformation("Deleted user ID {UserId}", userId);
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user ID {UserId}", userId);
            return false;
        }
    }

    public async Task RecordLoginAttemptAsync(int userId, bool success)
    {
        if (success)
        {
            const string sql = @"
                UPDATE Web.Users
                SET LastLoginDate = GETUTCDATE(),
                    FailedLoginAttempts = 0,
                    LockoutEnd = NULL,
                    ModifiedDate = GETUTCDATE()
                WHERE UserId = @UserId";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }
        else
        {
            // Increment failed attempts and potentially lock out
            var user = await GetUserByIdAsync(userId);
            if (user == null) return;

            var newFailedCount = user.FailedLoginAttempts + 1;
            DateTime? lockoutEnd = null;

            if (newFailedCount >= _authConfig.LockoutThreshold)
            {
                lockoutEnd = DateTime.UtcNow.AddMinutes(_authConfig.LockoutMinutes);
                _logger.LogWarning("User {Username} locked out until {LockoutEnd}",
                    user.Username, lockoutEnd);
            }

            const string sql = @"
                UPDATE Web.Users
                SET FailedLoginAttempts = @FailedCount,
                    LockoutEnd = @LockoutEnd,
                    ModifiedDate = GETUTCDATE()
                WHERE UserId = @UserId";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                FailedCount = newFailedCount,
                LockoutEnd = lockoutEnd
            });
        }
    }

    public async Task ResetFailedAttemptsAsync(int userId)
    {
        const string sql = @"
            UPDATE Web.Users
            SET FailedLoginAttempts = 0,
                LockoutEnd = NULL,
                ModifiedDate = GETUTCDATE()
            WHERE UserId = @UserId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    #region Password Hashing (PBKDF2-SHA256)

    /// <summary>
    /// Generate a cryptographically secure random salt
    /// </summary>
    private static string GenerateSalt()
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    /// <summary>
    /// Hash password using PBKDF2-SHA256 with provided salt
    /// </summary>
    private static string HashPassword(string password, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);
        return Convert.ToBase64String(hash);
    }

    #endregion
}
