namespace AiGroupChat.Email.Templates;

/// <summary>
/// Service for loading and rendering email templates
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Render the email confirmation template
    /// </summary>
    Task<(string Html, string Text)> RenderConfirmationEmailAsync(string userName, string confirmationUrl);
    
    /// <summary>
    /// Render the password reset template
    /// </summary>
    Task<(string Html, string Text)> RenderPasswordResetEmailAsync(string userName, string resetUrl);
}