using System.ComponentModel.DataAnnotations;

namespace AiGroupChat.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; } = string.Empty;
}