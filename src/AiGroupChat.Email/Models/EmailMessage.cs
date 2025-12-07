namespace AiGroupChat.Email.Models;

public class EmailMessage
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string To { get; set; } = string.Empty;
    
    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// HTML body content
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;
    
    /// <summary>
    /// Plain text body (fallback for email clients that don't support HTML)
    /// </summary>
    public string? TextBody { get; set; }
}