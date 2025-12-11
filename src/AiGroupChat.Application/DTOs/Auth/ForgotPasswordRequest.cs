using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to send a password reset email.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the account to reset.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}