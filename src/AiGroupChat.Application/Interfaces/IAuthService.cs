using AiGroupChat.Application.DTOs.Auth;

namespace AiGroupChat.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Register a new user and send confirmation email
    /// </summary>
    Task<MessageResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate user and return tokens
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm user's email address and return tokens
    /// </summary>
    Task<AuthResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend confirmation email to user
    /// </summary>
    Task<MessageResponse> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset user's password with token
    /// </summary>
    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    Task<MessageResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}