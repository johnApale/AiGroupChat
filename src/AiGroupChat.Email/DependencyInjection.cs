using AiGroupChat.Application.Interfaces;
using AiGroupChat.Email.Configuration;
using AiGroupChat.Email.Interfaces;
using AiGroupChat.Email.Providers;
using AiGroupChat.Email.Services;
using AiGroupChat.Email.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace AiGroupChat.Email;

public static class DependencyInjection
{
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        
        // Get API key for Resend
        EmailSettings? emailSettings = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>();
        
        // Register Resend client
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = emailSettings?.ApiKey ?? string.Empty;
        });
        services.AddTransient<IResend, ResendClient>();
        
        // Register email services
        services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailProvider, ResendEmailProvider>();
        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
}