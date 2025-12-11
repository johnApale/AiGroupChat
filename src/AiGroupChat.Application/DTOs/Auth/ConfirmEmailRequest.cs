using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to confirm email address using token from email.
/// </summary>
public class ConfirmEmailRequest
{
    /// <summary>
    /// Email address to confirm.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation token from the email link.
    /// </summary>
    /// <example>Q2ZESjhBT0... (base64 encoded)</example>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}