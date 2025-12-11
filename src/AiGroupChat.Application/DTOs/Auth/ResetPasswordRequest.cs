using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to reset password using token from email.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Email address of the account.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password reset token from the email link.
    /// </summary>
    /// <example>Q2ZESjhBT0... (base64 encoded)</example>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password. Must meet password requirements.
    /// </summary>
    /// <example>NewSecurePass456!</example>
    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; } = string.Empty;
}