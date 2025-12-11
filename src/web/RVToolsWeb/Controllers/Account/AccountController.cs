namespace RVToolsWeb.Controllers.Account;

using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly ICredentialProtectionService _credentialProtection;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        IUserService userService,
        ICredentialProtectionService credentialProtection,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _userService = userService;
        _credentialProtection = credentialProtection;
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

        string? generatedPassword = null;

        // Create admin user only for LocalDB provider
        // For LDAP, users authenticate against AD and are created on first login
        if (model.AuthProvider == "LocalDB")
        {
            // Generate a cryptographically secure random password
            generatedPassword = GenerateSecurePassword(16);

            // Create default admin user with generated password and force change flag
            var adminCreated = await _userService.CreateUserAsync(
                username: "admin",
                password: generatedPassword,
                role: "Admin",
                email: model.AdminEmail,
                forcePasswordChange: true);

            if (!adminCreated)
            {
                ModelState.AddModelError("", "Failed to create admin user. Please check database connectivity.");
                return View("Setup", model);
            }

            _logger.LogWarning("First-time setup completed with LocalDB. Admin user created with generated password.");
        }
        else
        {
            _logger.LogWarning("First-time setup completed with LDAP/AD authentication.");
        }

        // Mark setup as complete
        await _authService.CompleteSetupAsync(
            model.AuthProvider,
            model.LdapServer,
            model.LdapDomain,
            model.LdapBaseDN);

        // Reset middleware cache
        FirstTimeSetupMiddleware.ResetSetupCache();

        // Store setup completion flag and generated password (if LocalDB)
        TempData["SetupComplete"] = true;
        TempData["AuthProvider"] = model.AuthProvider;
        if (!string.IsNullOrEmpty(generatedPassword))
        {
            TempData["GeneratedPassword"] = generatedPassword;
        }
        return RedirectToAction("SetupComplete");
    }

    /// <summary>
    /// Display setup complete page with generated credentials
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult SetupComplete()
    {
        // Only show if we just completed setup
        if (TempData["SetupComplete"] == null)
        {
            return RedirectToAction("Login");
        }

        var authProvider = TempData["AuthProvider"]?.ToString();
        var generatedPassword = TempData["GeneratedPassword"]?.ToString();

        // For LocalDB, password is required
        // For LDAP, no password is generated (users auth against AD)
        if (authProvider == "LocalDB" && string.IsNullOrEmpty(generatedPassword))
        {
            return RedirectToAction("Login");
        }

        // Pass to view - shows password for LocalDB, instructions for LDAP
        ViewBag.GeneratedPassword = generatedPassword;
        ViewBag.AuthProvider = authProvider;
        return View();
    }

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    private static string GenerateSecurePassword(int length)
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // Excluding I, O
        const string lowercase = "abcdefghjkmnpqrstuvwxyz"; // Excluding i, l, o
        const string digits = "23456789"; // Excluding 0, 1
        const string special = "!@#$%^&*";
        const string allChars = uppercase + lowercase + digits + special;

        var password = new char[length];
        var randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Ensure at least one of each required character type
        password[0] = uppercase[randomBytes[0] % uppercase.Length];
        password[1] = lowercase[randomBytes[1] % lowercase.Length];
        password[2] = digits[randomBytes[2] % digits.Length];
        password[3] = special[randomBytes[3] % special.Length];

        // Fill the rest with random characters
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[randomBytes[i] % allChars.Length];
        }

        // Shuffle the password to avoid predictable positions
        return ShuffleString(new string(password));
    }

    /// <summary>
    /// Cryptographically shuffles a string.
    /// </summary>
    private static string ShuffleString(string input)
    {
        var chars = input.ToCharArray();
        var randomBytes = new byte[chars.Length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Fisher-Yates shuffle with cryptographic randomness
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = randomBytes[i] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
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
            // Create a secure, time-limited token instead of using TempData
            var token = _credentialProtection.CreatePasswordResetToken(user.UserId, user.Username);
            return RedirectToAction("ChangePassword", new { token });
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
    public IActionResult ChangePassword(string? token = null)
    {
        var model = new ChangePasswordViewModel();

        // Check if this is a forced password change via secure token
        if (!string.IsNullOrEmpty(token))
        {
            var tokenData = _credentialProtection.ValidatePasswordResetToken(token);
            if (tokenData == null)
            {
                // Token is invalid or expired
                _logger.LogWarning("Invalid or expired password reset token used");
                TempData["ErrorMessage"] = "Your password reset link has expired. Please log in again.";
                return RedirectToAction("Login");
            }

            model.IsForced = true;
            model.ResetToken = token; // Pass token to form for POST
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

        // Determine user ID from secure token or authenticated user
        if (!string.IsNullOrEmpty(model.ResetToken))
        {
            var tokenData = _credentialProtection.ValidatePasswordResetToken(model.ResetToken);
            if (tokenData == null)
            {
                // Token is invalid or expired
                _logger.LogWarning("Invalid or expired password reset token used in POST");
                TempData["ErrorMessage"] = "Your password reset link has expired. Please log in again.";
                return RedirectToAction("Login");
            }
            userId = tokenData.Value.UserId;
            username = tokenData.Value.Username;
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
