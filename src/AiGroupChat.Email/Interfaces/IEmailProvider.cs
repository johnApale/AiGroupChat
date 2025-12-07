using AiGroupChat.Application.Models;
using AiGroupChat.Email.Models;

namespace AiGroupChat.Email.Interfaces;

/// <summary>
/// Abstraction for email sending providers (Resend, Mailgun, SendGrid, etc.)
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Send an email through the provider
    /// </summary>
    Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}