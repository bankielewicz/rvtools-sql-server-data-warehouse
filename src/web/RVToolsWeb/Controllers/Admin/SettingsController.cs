using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Models.ViewModels.Admin;
using RVToolsWeb.Services.Admin;
using RVToolsWeb.Services.Auth;

namespace RVToolsWeb.Controllers.Admin;

/// <summary>
/// Administration/Settings controller for managing configuration.
/// </summary>
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly ITableRetentionService _retentionService;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IDatabaseStatusService _statusService;
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILdapService _ldapService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        ITableRetentionService retentionService,
        IAppSettingsService appSettingsService,
        IDatabaseStatusService statusService,
        IAuthService authService,
        IUserService userService,
        ILdapService ldapService,
        IWebHostEnvironment environment,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _retentionService = retentionService;
        _appSettingsService = appSettingsService;
        _statusService = statusService;
        _authService = authService;
        _userService = userService;
        _ldapService = ldapService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Display the settings page with all tabs.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tab = null)
    {
        // Get auth settings and users
        var authSettings = await _authService.GetAuthSettingsAsync();
        var users = await _userService.GetAllUsersAsync();

        var viewModel = new SettingsIndexViewModel
        {
            ActiveTab = tab ?? "general",
            GeneralSettings = await _settingsService.GetAllSettingsAsync(),
            TableRetentions = await _retentionService.GetAllRetentionsAsync(),
            AvailableTables = await _retentionService.GetAvailableTablesAsync(),
            AppSettings = await _appSettingsService.GetAppSettingsAsync(),
            DatabaseStatus = await _statusService.GetStatusAsync(),
            TableMappings = await _statusService.GetTableMappingsAsync(),
            TotalColumnMappings = await _statusService.GetColumnMappingCountAsync(),
            Environment = new EnvironmentViewModel
            {
                CurrentEnvironment = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsProduction = _environment.IsProduction(),
                ContentRootPath = _environment.ContentRootPath,
                WebRootPath = _environment.WebRootPath
            },
            Security = new SecurityViewModel
            {
                AuthSettings = new AuthSettingsViewModel
                {
                    AuthProvider = authSettings?.AuthProvider ?? "LocalDB",
                    LdapServer = authSettings?.LdapServer,
                    LdapDomain = authSettings?.LdapDomain,
                    LdapBaseDN = authSettings?.LdapBaseDN,
                    LdapPort = authSettings?.LdapPort ?? 389,
                    LdapUseSsl = authSettings?.LdapUseSsl ?? false,
                    LdapBindDN = authSettings?.LdapBindDN,
                    LdapAdminGroup = authSettings?.LdapAdminGroup,
                    LdapUserGroup = authSettings?.LdapUserGroup,
                    LdapFallbackToLocal = authSettings?.LdapFallbackToLocal ?? true,
                    IsConfigured = authSettings?.IsConfigured ?? false
                },
                Users = users.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ForcePasswordChange = u.ForcePasswordChange,
                    LastLoginDate = u.LastLoginDate,
                    CreatedDate = u.CreatedDate
                }).ToList()
            }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Update a Config.Settings value via AJAX.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.SettingName))
        {
            return BadRequest(new { success = false, error = "Setting name is required" });
        }

        var success = await _settingsService.UpdateSettingAsync(request.SettingName, request.SettingValue ?? "");
        return Json(new { success });
    }

    /// <summary>
    /// Add a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRetention([FromBody] AddRetentionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.FullTableName))
        {
            return BadRequest(new { success = false, error = "Table name is required" });
        }

        // Parse schema.table from FullTableName
        var parts = request.FullTableName.Split('.', 2);
        if (parts.Length != 2)
        {
            return BadRequest(new { success = false, error = "Invalid table name format. Expected Schema.TableName" });
        }

        var success = await _retentionService.AddRetentionAsync(parts[0], parts[1], request.RetentionDays);
        return Json(new { success });
    }

    /// <summary>
    /// Update a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRetention([FromBody] UpdateRetentionRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { success = false, error = "Valid retention ID is required" });
        }

        var success = await _retentionService.UpdateRetentionAsync(request.Id, request.RetentionDays);
        return Json(new { success });
    }

    /// <summary>
    /// Delete a table retention override.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRetention([FromBody] DeleteRetentionRequest request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest(new { success = false, error = "Valid retention ID is required" });
        }

        var success = await _retentionService.DeleteRetentionAsync(request.Id);
        return Json(new { success });
    }

    /// <summary>
    /// Update appsettings.json values.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppSettings([FromBody] AppSettingsViewModel settings)
    {
        if (settings == null)
        {
            return BadRequest(new { success = false, error = "Settings data is required" });
        }

        var (success, error) = await _appSettingsService.UpdateAppSettingsAsync(settings);

        if (success)
        {
            TempData["SettingsSuccess"] = "Application settings updated. Some changes may require an application restart to take effect.";
        }

        return Json(new { success, error });
    }

    /// <summary>
    /// Refresh database status (for AJAX polling).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RefreshStatus()
    {
        var status = await _statusService.GetStatusAsync();
        return PartialView("_DatabaseStatusPartial", status);
    }

    #region User Management

    /// <summary>
    /// Create a new user account.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser([FromBody] AddUserRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, error = "Username and password are required" });
        }

        var success = await _userService.CreateUserAsync(
            request.Username,
            request.Password,
            request.Role ?? "User",
            request.Email,
            request.ForcePasswordChange);

        if (!success)
        {
            return Json(new { success = false, error = "Failed to create user. Username may already exist." });
        }

        _logger.LogInformation("Admin created new user: {Username}", request.Username);
        return Json(new { success = true });
    }

    /// <summary>
    /// Update user account details.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
    {
        if (request == null || request.UserId <= 0)
        {
            return BadRequest(new { success = false, error = "Valid user ID is required" });
        }

        var success = await _userService.UpdateUserAsync(
            request.UserId,
            request.Email,
            request.Role ?? "User",
            request.IsActive);

        if (!success)
        {
            return Json(new { success = false, error = "Failed to update user" });
        }

        _logger.LogInformation("Admin updated user ID: {UserId}", request.UserId);
        return Json(new { success = true });
    }

    /// <summary>
    /// Reset user password (admin function).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (request == null || request.UserId <= 0 || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { success = false, error = "Valid user ID and new password are required" });
        }

        var success = await _userService.ResetPasswordAsync(
            request.UserId,
            request.NewPassword,
            request.ForceChange);

        if (!success)
        {
            return Json(new { success = false, error = "Failed to reset password" });
        }

        _logger.LogInformation("Admin reset password for user ID: {UserId}", request.UserId);
        return Json(new { success = true });
    }

    /// <summary>
    /// Delete a user account.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
    {
        if (request == null || request.UserId <= 0)
        {
            return BadRequest(new { success = false, error = "Valid user ID is required" });
        }

        var success = await _userService.DeleteUserAsync(request.UserId);

        if (!success)
        {
            return Json(new { success = false, error = "Failed to delete user. Cannot delete admin account." });
        }

        _logger.LogInformation("Admin deleted user ID: {UserId}", request.UserId);
        return Json(new { success = true });
    }

    #endregion

    #region Environment Management

    /// <summary>
    /// Switch ASPNETCORE_ENVIRONMENT by modifying web.config.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchEnvironment([FromBody] SwitchEnvironmentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Environment))
        {
            return BadRequest(new { success = false, error = "Environment name is required" });
        }

        try
        {
            var webConfigPath = Path.Combine(_environment.ContentRootPath, "web.config");

            // Check if web.config exists (IIS deployment)
            if (!System.IO.File.Exists(webConfigPath))
            {
                return Json(new { success = false, error = "web.config not found. This feature requires IIS deployment." });
            }

            // Read web.config
            var webConfigContent = await System.IO.File.ReadAllTextAsync(webConfigPath);

            // Replace environment variable value
            var updatedContent = System.Text.RegularExpressions.Regex.Replace(
                webConfigContent,
                @"(<environmentVariable\s+name=""ASPNETCORE_ENVIRONMENT""\s+value="")[^""]*(""\s*/>)",
                $"$1{request.Environment}$2",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // If no environment variable exists, add it
            if (!webConfigContent.Contains("ASPNETCORE_ENVIRONMENT"))
            {
                updatedContent = updatedContent.Replace(
                    "</environmentVariables>",
                    $"    <environmentVariable name=\"ASPNETCORE_ENVIRONMENT\" value=\"{request.Environment}\" />\n      </environmentVariables>");
            }

            // Write updated web.config (this will trigger app pool recycle)
            await System.IO.File.WriteAllTextAsync(webConfigPath, updatedContent);

            _logger.LogWarning("Environment switched to {Environment} via web.config modification", request.Environment);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch environment");
            return Json(new { success = false, error = "Failed to modify web.config: " + ex.Message });
        }
    }

    #endregion

    #region LDAP Configuration

    /// <summary>
    /// Test LDAP connection with provided settings.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestLdapConnection([FromBody] TestLdapRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LdapServer) || string.IsNullOrWhiteSpace(request.LdapBaseDN))
        {
            return BadRequest(new { success = false, error = "LDAP server and Base DN are required" });
        }

        var (success, message) = await _ldapService.TestConnectionAsync(
            request.LdapServer,
            request.LdapPort,
            request.LdapUseSsl,
            request.LdapBaseDN,
            request.LdapBindDN,
            request.LdapBindPassword);

        return Json(new { success, message });
    }

    /// <summary>
    /// Update LDAP configuration settings.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLdapSettings([FromBody] UpdateLdapSettingsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LdapServer) || string.IsNullOrWhiteSpace(request.LdapBaseDN))
        {
            return BadRequest(new { success = false, error = "LDAP server and Base DN are required" });
        }

        var success = await _authService.UpdateLdapSettingsAsync(
            request.LdapServer,
            request.LdapDomain,
            request.LdapBaseDN,
            request.LdapPort,
            request.LdapUseSsl,
            request.LdapBindDN,
            request.LdapBindPassword,
            request.LdapAdminGroup,
            request.LdapUserGroup,
            request.LdapFallbackToLocal,
            request.LdapValidateCertificate,
            request.LdapCertificateThumbprint);

        if (success)
        {
            _logger.LogInformation("LDAP settings updated by admin");
        }

        return Json(new { success });
    }

    /// <summary>
    /// Switch authentication provider between LocalDB and LDAP.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchAuthProvider([FromBody] SwitchAuthProviderRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.AuthProvider))
        {
            return BadRequest(new { success = false, error = "Auth provider is required" });
        }

        if (request.AuthProvider != "LocalDB" && request.AuthProvider != "LDAP")
        {
            return BadRequest(new { success = false, error = "Invalid auth provider. Must be 'LocalDB' or 'LDAP'" });
        }

        var success = await _authService.UpdateAuthSettingsAsync(request.AuthProvider);

        if (success)
        {
            _logger.LogWarning("Authentication provider switched to {Provider} by admin", request.AuthProvider);
        }

        return Json(new { success });
    }

    #endregion
}

// Request DTOs for AJAX operations
public class UpdateSettingRequest
{
    public string? SettingName { get; set; }
    public string? SettingValue { get; set; }
}

public class AddRetentionRequest
{
    public string? FullTableName { get; set; }
    public int RetentionDays { get; set; }
}

public class UpdateRetentionRequest
{
    public int Id { get; set; }
    public int RetentionDays { get; set; }
}

public class DeleteRetentionRequest
{
    public int Id { get; set; }
}

public class AddUserRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool ForcePasswordChange { get; set; }
}

public class UpdateUserRequest
{
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
}

public class ResetPasswordRequest
{
    public int UserId { get; set; }
    public string? NewPassword { get; set; }
    public bool ForceChange { get; set; }
}

public class DeleteUserRequest
{
    public int UserId { get; set; }
}

public class SwitchEnvironmentRequest
{
    public string? Environment { get; set; }
}

public class TestLdapRequest
{
    public string? LdapServer { get; set; }
    public int LdapPort { get; set; } = 389;
    public bool LdapUseSsl { get; set; }
    public string? LdapBaseDN { get; set; }
    public string? LdapBindDN { get; set; }
    public string? LdapBindPassword { get; set; }
}

public class UpdateLdapSettingsRequest
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

public class SwitchAuthProviderRequest
{
    public string? AuthProvider { get; set; }
}
