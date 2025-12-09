using AiGroupChat.Application.Models;
using AiGroupChat.Email.Interfaces;
using AiGroupChat.Email.Models;

namespace AiGroupChat.IntegrationTests.Infrastructure;

/// <summary>
/// Fake email provider for integration tests.
/// Captures sent emails in memory instead of actually sending them.
/// </summary>
public class FakeEmailProvider : IEmailProvider
{
    private readonly List<EmailMessage> _sentEmails = new();
    private readonly object _lock = new();

    /// <summary>
    /// Gets all emails that have been "sent" during tests
    /// </summary>
    public IReadOnlyList<EmailMessage> SentEmails
    {
        get
        {
            lock (_lock)
            {
                return _sentEmails.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the most recently sent email, or null if none have been sent
    /// </summary>
    public EmailMessage? LastEmail
    {
        get
        {
            lock (_lock)
            {
                return _sentEmails.LastOrDefault();
            }
        }
    }

    public Task<EmailResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _sentEmails.Add(message);
        }

        return Task.FromResult(EmailResult.Success($"fake-email-{Guid.NewGuid()}"));
    }

    /// <summary>
    /// Clears all captured emails (useful between tests)
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _sentEmails.Clear();
        }
    }

    /// <summary>
    /// Extracts a token from the last email's HTML body.
    /// Looks for token in query string format: ?token={value} or &token={value}
    /// </summary>
    public string? ExtractTokenFromLastEmail()
    {
        string? htmlBody = LastEmail?.HtmlBody;
        if (string.IsNullOrEmpty(htmlBody))
            return null;

        // Look for token parameter in URL
        int tokenIndex = htmlBody.IndexOf("token=", StringComparison.OrdinalIgnoreCase);
        if (tokenIndex == -1)
            return null;

        int startIndex = tokenIndex + "token=".Length;
        int endIndex = htmlBody.IndexOfAny(new[] { '&', '"', '<', ' ', '\n', '\r' }, startIndex);
        
        if (endIndex == -1)
            endIndex = htmlBody.Length;

        string token = htmlBody.Substring(startIndex, endIndex - startIndex);
        
        // URL decode the token
        return Uri.UnescapeDataString(token);
    }
}