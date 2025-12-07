namespace AiGroupChat.Email.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";
    
    /// <summary>
    /// Resend API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Sender email address (must be verified in Resend)
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Sender display name
    /// </summary>
    public string FromName { get; set; } = "AI Group Chat";
    
    /// <summary>
    /// Frontend base URL for email links
    /// </summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Path for email confirmation page
    /// </summary>
    public string ConfirmEmailPath { get; set; } = "/confirm-email";
    
    /// <summary>
    /// Path for password reset page
    /// </summary>
    public string ResetPasswordPath { get; set; } = "/reset-password";
}