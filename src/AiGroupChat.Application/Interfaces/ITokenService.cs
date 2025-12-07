using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface ITokenService
{
    /// <summary>
    /// Generate a JWT access token for the user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate and store a refresh token for the user
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate a refresh token and return the associated user ID if valid
    /// </summary>
    Task<string?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a specific refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke all refresh tokens for a user (e.g., after password reset)
    /// </summary>
    Task RevokeAllUserRefreshTokensAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the access token expiration time
    /// </summary>
    DateTime GetAccessTokenExpiration();
}