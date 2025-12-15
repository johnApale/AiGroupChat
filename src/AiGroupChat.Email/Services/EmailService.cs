using System.Web;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Models;
using AiGroupChat.Email.Configuration;
using AiGroupChat.Email.Interfaces;
using AiGroupChat.Email.Models;
using AiGroupChat.Email.Templates;
using Microsoft.Extensions.Options;

namespace AiGroupChat.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailProvider _emailProvider;
    private readonly IEmailTemplateService _templateService;
    private readonly EmailSettings _settings;

    public EmailService(
        IEmailProvider emailProvider, 
        IEmailTemplateService templateService,
        IOptions<EmailSettings> settings)
    {
        _emailProvider = emailProvider;
        _templateService = templateService;
        _settings = settings.Value;
    }

    public async Task<EmailResult> SendConfirmationEmailAsync(
        string toEmail, 
        string userName, 
        string confirmationToken, 
        CancellationToken cancellationToken = default)
    {
        string confirmationUrl = BuildUrl(_settings.ConfirmEmailPath, confirmationToken, toEmail);
        (string html, string text) = await _templateService.RenderConfirmationEmailAsync(userName, confirmationUrl);
        
        EmailMessage message = new EmailMessage
        {
            To = toEmail,
            Subject = "Confirm your email - AI Group Chat",
            HtmlBody = html,
            TextBody = text
        };

        return await _emailProvider.SendAsync(message, cancellationToken);
    }

    public async Task<EmailResult> SendPasswordResetEmailAsync(
        string toEmail, 
        string userName, 
        string resetToken, 
        CancellationToken cancellationToken = default)
    {
        string resetUrl = BuildUrl(_settings.ResetPasswordPath, resetToken, toEmail);
        (string html, string text) = await _templateService.RenderPasswordResetEmailAsync(userName, resetUrl);
        
        EmailMessage message = new EmailMessage
        {
            To = toEmail,
            Subject = "Reset your password - AI Group Chat",
            HtmlBody = html,
            TextBody = text
        };

        return await _emailProvider.SendAsync(message, cancellationToken);
    }

    public async Task<EmailResult> SendGroupInvitationEmailAsync(
        string toEmail,
        string groupName,
        string inviterName,
        string invitationToken,
        int expirationDays,
        CancellationToken cancellationToken = default)
    {
        string invitationUrl = BuildInvitationUrl(invitationToken);
        (string html, string text) = await _templateService.RenderGroupInvitationEmailAsync(
            groupName, inviterName, invitationUrl, expirationDays);
        
        EmailMessage message = new EmailMessage
        {
            To = toEmail,
            Subject = $"You're invited to join {groupName} - AI Group Chat",
            HtmlBody = html,
            TextBody = text
        };

        return await _emailProvider.SendAsync(message, cancellationToken);
    }

    private string BuildUrl(string path, string token, string email)
    {
        string encodedToken = HttpUtility.UrlEncode(token);
        string encodedEmail = HttpUtility.UrlEncode(email);
        return $"{_settings.FrontendBaseUrl}{path}?token={encodedToken}&email={encodedEmail}";
    }

    private string BuildInvitationUrl(string token)
    {
        string encodedToken = HttpUtility.UrlEncode(token);
        return $"{_settings.FrontendBaseUrl}{_settings.AcceptInvitationPath}?token={encodedToken}";
    }
}