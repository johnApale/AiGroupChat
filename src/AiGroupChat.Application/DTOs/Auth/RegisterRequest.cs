using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to create a new user account.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address. Must be unique. Used for login and notifications.
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Unique username. 3-50 characters. Used for display and @mentions.
    /// </summary>
    /// <example>johndoe</example>
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown in the UI. Up to 100 characters.
    /// </summary>
    /// <example>John Doe</example>
    [Required(ErrorMessage = "Display name is required")]
    [MaxLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Password. Must contain uppercase, lowercase, digit, and special character.
    /// </summary>
    /// <example>SecurePass123!</example>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional invitation token. When provided, email is auto-confirmed 
    /// and user is added to the group. The email must match the invitation.
    /// </summary>
    /// <example>abc123-secure-token</example>
    public string? InviteToken { get; set; }
}