# AiGroupChat.Email

Email service library for the AI Group Chat application.

## Overview

This project provides a provider-agnostic email service with support for:

- Email confirmation emails
- Password reset emails
- HTML templates with plain-text fallbacks

## Architecture

The interface (`IEmailService`) is defined in the **Application** layer to maintain Clean Architecture principles. This project provides the implementation.

```
Application Layer
├── Interfaces/
│   └── IEmailService.cs       # Interface (abstraction)
├── Models/
│   └── EmailResult.cs         # Result model

Email Project (this project)
├── Configuration/
│   └── EmailSettings.cs       # Configuration model
├── Interfaces/
│   └── IEmailProvider.cs      # Internal provider abstraction
├── Models/
│   └── EmailMessage.cs        # Internal email message model
├── Providers/
│   └── ResendEmailProvider.cs # Resend implementation
├── Services/
│   └── EmailService.cs        # IEmailService implementation
├── Templates/
│   ├── Html/
│   │   ├── ConfirmEmail.html  # Confirmation email template
│   │   └── PasswordReset.html # Password reset template
│   ├── IEmailTemplateService.cs
│   └── EmailTemplateService.cs
└── DependencyInjection.cs     # DI registration
```

## Configuration

Add to `appsettings.json`:

```json
{
  "Email": {
    "ApiKey": "re_xxxxxxxx",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "AI Group Chat",
    "FrontendBaseUrl": "https://app.yourdomain.com",
    "ConfirmEmailPath": "/confirm-email",
    "ResetPasswordPath": "/reset-password"
  }
}
```

## Usage

Register in `Program.cs` or DI setup:

```csharp
services.AddEmail(configuration);
```

Inject and use:

```csharp
public class AuthService
{
    private readonly IEmailService _emailService;

    public AuthService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendConfirmation(string email, string name, string token)
    {
        var result = await _emailService.SendConfirmationEmailAsync(email, name, token);

        if (!result.IsSuccess)
        {
            // Handle error
        }
    }
}
```

## Switching Providers

To switch from Resend to another provider (e.g., Mailgun):

1. Create `MailgunEmailProvider : IEmailProvider`
2. Update `DependencyInjection.cs` to register the new provider

```csharp
services.AddScoped<IEmailProvider, MailgunEmailProvider>();
```

## Design Decisions

1. **Interface in Application layer** - `IEmailService` is defined in the Application layer so that business logic (like `AuthService`) depends on an abstraction, not a concrete implementation. This follows the Dependency Inversion Principle.

2. **EmailResult in Application layer** - Since it's the return type of `IEmailService`, it must also be in the Application layer.

3. **Internal IEmailProvider** - The provider abstraction (`IEmailProvider`) stays in this project since it's an internal implementation detail, not used by other layers.
