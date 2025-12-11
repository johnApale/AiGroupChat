using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to authenticate and receive tokens.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email address used during registration.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Account password.
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}