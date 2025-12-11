using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

/// <summary>
/// Request to revoke a refresh token.
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// Refresh token to revoke.
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}