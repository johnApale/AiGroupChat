using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to resend the confirmation email.
/// </summary>
public class ResendConfirmationRequest
{
    /// <summary>
    /// Email address to send confirmation to.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}