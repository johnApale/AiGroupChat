using AiGroupChat.Application.Models;

namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// High-level email service for sending application emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email confirmation link to a new user
    /// </summary>
    Task<EmailResult> SendConfirmationEmailAsync(string toEmail, string userName, string confirmationToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send password reset link to a user
    /// </summary>
    Task<EmailResult> SendPasswordResetEmailAsync(string toEmail, string userName, string resetToken, CancellationToken cancellationToken = default);
}