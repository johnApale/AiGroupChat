using AiGroupChat.Application.Models;
using AiGroupChat.Email.Configuration;
using AiGroupChat.Email.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace AiGroupChat.Email.Providers;

public class ResendEmailProvider : IEmailProvider
{
    private readonly IResend _resend;
    private readonly EmailSettings _settings;
    private readonly ILogger<ResendEmailProvider> _logger;

    public ResendEmailProvider(
        IResend resend,
        IOptions<EmailSettings> settings,
        ILogger<ResendEmailProvider> logger)
    {
        _resend = resend;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(Models.EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            Resend.EmailMessage resendMessage = new Resend.EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                To = { message.To },
                Subject = message.Subject,
                HtmlBody = message.HtmlBody,
                TextBody = message.TextBody
            };

            ResendResponse<Guid> response = await _resend.EmailSendAsync(resendMessage, cancellationToken);
            string emailId = response.Content.ToString();

            _logger.LogInformation("Email sent successfully to {Recipient}. EmailId: {EmailId}", 
                message.To, emailId);

            return EmailResult.Success(emailId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", message.To);
            return EmailResult.Failure(ex.Message);
        }
    }
}