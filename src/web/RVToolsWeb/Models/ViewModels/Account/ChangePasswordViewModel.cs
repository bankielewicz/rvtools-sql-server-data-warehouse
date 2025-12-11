namespace RVToolsWeb.Models.ViewModels.Account;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// View model for password change form
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// True when user is required to change password (first login)
    /// </summary>
    public bool IsForced { get; set; }

    /// <summary>
    /// Secure, time-limited token for forced password reset.
    /// Contains encrypted user ID and expiration time.
    /// </summary>
    public string? ResetToken { get; set; }
}
