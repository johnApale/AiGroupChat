using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Find a user by their email address
    /// </summary>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find a user by their ID
    /// </summary>
    Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new user with password
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> CreateAsync(User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate user's password
    /// </summary>
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user's email is confirmed
    /// </summary>
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate email confirmation token
    /// </summary>
    Task<string> GenerateEmailConfirmationTokenAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm user's email with token
    /// </summary>
    Task<bool> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm user's email directly without token (for invite-based registration)
    /// </summary>
    Task ConfirmEmailDirectAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate password reset token
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset user's password with token
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default);
}