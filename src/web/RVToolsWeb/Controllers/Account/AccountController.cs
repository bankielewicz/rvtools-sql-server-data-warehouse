namespace RVToolsWeb.Controllers.Account;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVToolsWeb.Middleware;
using RVToolsWeb.Models.ViewModels.Account;
using RVToolsWeb.Services.Auth;

/// <summary>
/// Controller for authentication and account management
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IUserService userService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    #region First-Time Setup

    /// <summary>
    /// Display first-time setup wizard
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        // Only show if setup is required
        if (!await _authService.IsSetupRequiredAsync())
        {
            return RedirectToAction("Login");
        }

        return View(new SetupViewModel());
    }

    /// <summary>
    /// Complete first-time setup
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSetup(SetupViewModel model)
    {
        if (!await _authService.IsSetupRequiredAsync())
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View("Setup", model);
        }

        // For MVP, only LocalDB is supported
        model.AuthProvider = "LocalDB";

        // Create default admin user with password 'admin' and force change flag
        var adminCreated = await _userService.CreateUserAsync(
            username: "admin",
            password: "admin",
            role: "Admin",
            email: model.AdminEmail,
            forcePasswordChange: true);

        if (!adminCreated)
        {
            ModelState.AddModelError("", "Failed to create admin user. Please check database connectivity.");
            return View("Setup", model);
        }

        // Mark setup as complete
        await _authService.CompleteSetupAsync(
            model.AuthProvider,
            model.LdapServer,
            model.LdapDomain,
            model.LdapBaseDN);

        // Reset middleware cache
        FirstTimeSetupMiddleware.ResetSetupCache();

        _logger.LogWarning("First-time setup completed. Admin user created with default password.");

        TempData["SetupComplete"] = true;
        return RedirectToAction("Login");
    }

    #endregion

    #region Login/Logout

    /// <summary>
    /// Display login form
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new LoginViewModel { ReturnUrl = returnUrl };

        // Show setup complete message
        if (TempData["SetupComplete"] != null)
        {
            ViewBag.SetupComplete = true;
        }

        return View(model);
    }

    /// <summary>
    /// Process login form
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userService.ValidateCredentialsAsync(model.Username, model.Password);

        if (user == null)
        {
            model.ErrorMessage = "Invalid username or password, or account is locked.";
            return View(model);
        }

        // Check if password change is required
        if (user.ForcePasswordChange)
        {
            TempData["ForceChangeUserId"] = user.UserId;
            TempData["ForceChangeUsername"] = user.Username;
            return RedirectToAction("ChangePassword");
        }

        // Sign in user
        await SignInUserAsync(user, model.RememberMe);

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        // Redirect to return URL or home
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Log out current user
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("User {Username} logged out", username);

        return RedirectToAction("Login");
    }

    #endregion

    #region Password Change

    /// <summary>
    /// Display password change form
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Allow for forced password change scenario
    public IActionResult ChangePassword()
    {
        var model = new ChangePasswordViewModel();

        // Check if this is a forced password change
        if (TempData["ForceChangeUserId"] != null)
        {
            model.IsForced = true;
            TempData.Keep("ForceChangeUserId");
            TempData.Keep("ForceChangeUsername");
            return View("ForcedChangePassword", model);
        }
        else if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Login");
        }

        return View(model);
    }

    /// <summary>
    /// Process password change
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous] // Allow for forced password change scenario
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        int userId;
        string username;

        // Determine user ID from forced change or authenticated user
        if (TempData["ForceChangeUserId"] is int forcedUserId)
        {
            userId = forcedUserId;
            username = TempData["ForceChangeUsername"]?.ToString() ?? "unknown";
            model.IsForced = true;
        }
        else if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out userId))
            {
                return RedirectToAction("Login");
            }
            username = User.Identity.Name ?? "unknown";
        }
        else
        {
            return RedirectToAction("Login");
        }

        // Validate current password
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var validPassword = await _userService.ValidateCredentialsAsync(user.Username, model.CurrentPassword);
        if (validPassword == null)
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
            return View(model);
        }

        // Prevent using same password
        if (model.CurrentPassword == model.NewPassword)
        {
            ModelState.AddModelError("NewPassword", "New password must be different from current password");
            return View(model);
        }

        // Update password
        var success = await _userService.UpdatePasswordAsync(userId, model.NewPassword);

        if (!success)
        {
            ModelState.AddModelError("", "Failed to update password. Please try again.");
            return View(model);
        }

        _logger.LogInformation("User {Username} changed their password", username);

        // If this was a forced change, redirect to login
        if (model.IsForced)
        {
            TempData["PasswordChanged"] = true;
            return RedirectToAction("Login");
        }

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Access Denied

    /// <summary>
    /// Display access denied page
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion

    #region Helpers

    private async Task SignInUserAsync(Models.DTOs.UserDto user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("AuthSource", user.AuthSource)
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    #endregion
}
