using System.Reflection;

namespace AiGroupChat.Email.Templates;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly Assembly _assembly;
    private readonly string _baseNamespace;

    public EmailTemplateService()
    {
        _assembly = typeof(EmailTemplateService).Assembly;
        _baseNamespace = "AiGroupChat.Email.Templates.Html";
    }

    public async Task<(string Html, string Text)> RenderConfirmationEmailAsync(string userName, string confirmationUrl)
    {
        var html = await LoadTemplateAsync("ConfirmEmail.html");
        
        html = html
            .Replace("{{UserName}}", userName)
            .Replace("{{ConfirmationUrl}}", confirmationUrl);

        var text = GenerateConfirmationTextEmail(userName, confirmationUrl);

        return (html, text);
    }

    public async Task<(string Html, string Text)> RenderPasswordResetEmailAsync(string userName, string resetUrl)
    {
        var html = await LoadTemplateAsync("PasswordReset.html");
        
        html = html
            .Replace("{{UserName}}", userName)
            .Replace("{{ResetUrl}}", resetUrl);

        var text = GeneratePasswordResetTextEmail(userName, resetUrl);

        return (html, text);
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var resourceName = $"{_baseNamespace}.{templateName}";
        
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            throw new FileNotFoundException($"Email template '{templateName}' not found. Resource name: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static string GenerateConfirmationTextEmail(string userName, string confirmationUrl)
    {
        return $@"Welcome to AI Group Chat, {userName}!

Thanks for signing up. Please confirm your email address by visiting the link below:

{confirmationUrl}

This link will expire in 24 hours.

If you didn't create an account, you can safely ignore this email.";
    }

    private static string GeneratePasswordResetTextEmail(string userName, string resetUrl)
    {
        return $@"Password Reset Request

Hi {userName},

We received a request to reset your password. Visit the link below to choose a new password:

{resetUrl}

This link will expire in 1 hour.

If you didn't request a password reset, you can safely ignore this email — your password will remain unchanged.";
    }
}