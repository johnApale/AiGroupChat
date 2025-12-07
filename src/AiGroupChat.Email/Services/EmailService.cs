using System.Web;
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
        var confirmationUrl = BuildUrl(_settings.ConfirmEmailPath, confirmationToken, toEmail);
        var (html, text) = await _templateService.RenderConfirmationEmailAsync(userName, confirmationUrl);
        
        var message = new EmailMessage
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
        var resetUrl = BuildUrl(_settings.ResetPasswordPath, resetToken, toEmail);
        var (html, text) = await _templateService.RenderPasswordResetEmailAsync(userName, resetUrl);
        
        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Reset your password - AI Group Chat",
            HtmlBody = html,
            TextBody = text
        };

        return await _emailProvider.SendAsync(message, cancellationToken);
    }

    private string BuildUrl(string path, string token, string email)
    {
        var encodedToken = HttpUtility.UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(email);
        return $"{_settings.FrontendBaseUrl}{path}?token={encodedToken}&email={encodedEmail}";
    }
}