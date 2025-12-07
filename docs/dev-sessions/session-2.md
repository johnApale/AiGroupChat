# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 1: Project Setup & Infrastructure

**Date:** December 6, 2025

### Completed

#### 1. Project Initialization

- Created .NET 9 solution with Clean Architecture structure
- Four projects established:
  - `AiGroupChat.Domain` - Core entities, no dependencies
  - `AiGroupChat.Application` - Business logic (empty, ready for services)
  - `AiGroupChat.Infrastructure` - EF Core, database access
  - `AiGroupChat.API` - Web API entry point
- Project references configured: API → Infrastructure → Application → Domain

#### 2. Domain Layer

- **Entities created:**

  - `User` (extends IdentityUser) - application user with DisplayName
  - `Group` - chat group with AI monitoring toggle
  - `GroupMember` - user membership with role (Admin/Member)
  - `Message` - chat message (user or AI) with attachment support
  - `AiProvider` - AI provider configuration (Gemini, Claude, etc.)
  - `AiResponseMetadata` - token usage, latency, cost tracking
  - `RefreshToken` - JWT refresh token storage

- **Enums created:**
  - `SenderType` - User, Ai
  - `GroupRole` - Member, Admin

#### 3. Infrastructure Layer

- **Database:** PostgreSQL 16 running in Docker (port 5434)
- **DbContext:** `ApplicationDbContext` with Identity integration
- **Entity Configurations:** All 7 entities configured with:
  - Snake_case table/column naming
  - Proper indexes and constraints
  - Foreign key relationships
  - Enum to string conversions
- **Dependency Injection:** `AddInfrastructure()` extension method

#### 4. API Layer

- **Framework:** ASP.NET Core 9 Web API
- **API Documentation:** Scalar (replaced default Swagger)
- **Configuration:** Connection strings in appsettings (Development.json gitignored)

#### 5. Database

- Initial migration created and applied
- All tables created in PostgreSQL:
  - `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc. (Identity)
  - `ai_providers`, `groups`, `group_members`, `messages`
  - `ai_response_metadata`, `refresh_tokens`

#### 6. Documentation

- Root `README.md` - project overview, setup instructions
- `src/AiGroupChat.Domain/README.md` - entity documentation
- `src/AiGroupChat.Infrastructure/README.md` - database documentation
- `.gitignore` configured for .NET, secrets, macOS

#### 7. Docker Setup

- `docker-compose.yml` for PostgreSQL development database
- Container name: `aigroupchat-db`
- Database: `aigroupchat_dev`
- Credentials: `aigroupchat` / `devpassword123`

---

## Session 2: Email Service & Authentication Planning

**Date:** December 6, 2025

### Completed

#### 1. Specification Document Updates

Updated `ai-group-chat-spec.md` from v1.0 to v1.1:

- Added Email Service (Resend) to tech stack
- Added token expiration times configuration:
  - Access Token: 15 minutes
  - Refresh Token: 7 days
  - Email Confirmation Token: 24 hours
  - Password Reset Token: 1 hour
- Added frontend URL configuration for email links
- Added new authentication endpoints:
  - `POST /api/auth/confirm-email`
  - `POST /api/auth/resend-confirmation`
  - `POST /api/auth/forgot-password`
  - `POST /api/auth/reset-password`
- Added full request/response contracts for all auth endpoints
- Added Email Verification Flow diagram
- Added Password Reset Flow diagram
- Updated architecture diagram to include Resend Email Service

#### 2. Email Project (`AiGroupChat.Email`)

Created a new standalone email project with provider abstraction:

**Project Structure:**
```
src/AiGroupChat.Email/
├── Configuration/
│   └── EmailSettings.cs           # Configuration model
├── Interfaces/
│   ├── IEmailProvider.cs          # Provider abstraction (swappable)
│   └── IEmailService.cs           # High-level email service
├── Models/
│   ├── EmailMessage.cs            # Email message model
│   └── EmailResult.cs             # Send result with success/failure
├── Providers/
│   └── ResendEmailProvider.cs     # Resend implementation
├── Services/
│   └── EmailService.cs            # Orchestrates template + sending
├── Templates/
│   ├── Html/
│   │   ├── ConfirmEmail.html      # Email confirmation template
│   │   └── PasswordReset.html     # Password reset template
│   ├── IEmailTemplateService.cs   # Template rendering interface
│   └── EmailTemplateService.cs    # Loads & renders HTML templates
├── DependencyInjection.cs         # Service registration
├── AiGroupChat.Email.csproj
└── README.md
```

**NuGet Packages:**
- `Resend` (0.2.1) - Resend email API client
- `Microsoft.Extensions.Logging.Abstractions` - Logging support
- `Microsoft.Extensions.Configuration.Binder` - Configuration binding
- `Microsoft.Extensions.Http` - HttpClient factory

**Key Design Decisions:**
- Provider-agnostic: Can swap Resend for Mailgun/SendGrid by implementing `IEmailProvider`
- HTML templates as embedded resources for clean deployment
- Plain-text fallback for all emails
- URL building handles token encoding automatically

#### 3. Infrastructure Integration

- Updated `DependencyInjection.cs` to call `AddEmail()`
- Added Email configuration section to `appsettings.json`

#### 4. Git Repository

- Initialized Git repository
- Created two commits:
  1. Session 1: Initial project setup with Clean Architecture
  2. Session 2: Email project with Resend integration
- Pushed to GitHub: https://github.com/johnApale/AiGroupChat

---

## Project Structure (Current)

```
AiGroupChat/
├── src/
│   ├── AiGroupChat.API/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json  # (gitignored)
│   │   └── AiGroupChat.API.csproj
│   │
│   ├── AiGroupChat.Application/
│   │   └── AiGroupChat.Application.csproj  # Empty, ready for services
│   │
│   ├── AiGroupChat.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Group.cs
│   │   │   ├── GroupMember.cs
│   │   │   ├── Message.cs
│   │   │   ├── AiProvider.cs
│   │   │   ├── AiResponseMetadata.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   ├── SenderType.cs
│   │   │   └── GroupRole.cs
│   │   └── README.md
│   │
│   ├── AiGroupChat.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Configurations/
│   │   │       └── [7 entity configurations]
│   │   ├── Migrations/
│   │   │   └── [InitialCreate migration files]
│   │   ├── DependencyInjection.cs
│   │   └── README.md
│   │
│   └── AiGroupChat.Email/              # NEW IN SESSION 2
│       ├── Configuration/
│       │   └── EmailSettings.cs
│       ├── Interfaces/
│       │   ├── IEmailProvider.cs
│       │   └── IEmailService.cs
│       ├── Models/
│       │   ├── EmailMessage.cs
│       │   └── EmailResult.cs
│       ├── Providers/
│       │   └── ResendEmailProvider.cs
│       ├── Services/
│       │   └── EmailService.cs
│       ├── Templates/
│       │   ├── Html/
│       │   │   ├── ConfirmEmail.html
│       │   │   └── PasswordReset.html
│       │   ├── IEmailTemplateService.cs
│       │   └── EmailTemplateService.cs
│       ├── DependencyInjection.cs
│       └── README.md
│
├── tests/                              # Empty, ready for test projects
├── docs/
│   └── DEVELOPMENT_PROGRESS.md         # This file
├── docker-compose.yml
├── .gitignore
├── README.md
└── AiGroupChat.sln
```

---

## What's Next: Session 3

### Phase 2: Authentication (Continue)

#### Step 2: Application Layer DTOs & Interfaces

Create auth DTOs in `src/AiGroupChat.Application/DTOs/Auth/`:

| File | Purpose |
|------|---------|
| `RegisterRequest.cs` | Registration input |
| `LoginRequest.cs` | Login input |
| `ConfirmEmailRequest.cs` | Email confirmation input |
| `ResendConfirmationRequest.cs` | Resend confirmation input |
| `ForgotPasswordRequest.cs` | Password reset request input |
| `ResetPasswordRequest.cs` | Password reset input |
| `RefreshTokenRequest.cs` | Token refresh input |
| `AuthResponse.cs` | Login/confirm response with tokens |
| `MessageResponse.cs` | Simple message response |

Create service interfaces in `src/AiGroupChat.Application/Interfaces/`:

| File | Purpose |
|------|---------|
| `IAuthService.cs` | Authentication business logic |
| `ITokenService.cs` | JWT token generation |

#### Step 3: Infrastructure Services

Implement in `src/AiGroupChat.Infrastructure/Services/`:

| File | Purpose |
|------|---------|
| `TokenService.cs` | JWT + refresh token implementation |

Add configuration:

| File | Purpose |
|------|---------|
| `JwtSettings.cs` | JWT configuration model |

#### Step 4: Application Services

Implement in `src/AiGroupChat.Application/Services/`:

| File | Purpose |
|------|---------|
| `AuthService.cs` | Registration, login, password reset logic |

#### Step 5: API Layer

Create in `src/AiGroupChat.API/Controllers/`:

| File | Purpose |
|------|---------|
| `AuthController.cs` | All auth endpoints |

Configure in `Program.cs`:
- ASP.NET Identity
- JWT authentication middleware
- Authorization policies

#### Step 6: Testing & Documentation

- Test all auth endpoints via Scalar
- Update `DEVELOPMENT_PROGRESS.md`

---

## Commands Reference

```bash
# Start PostgreSQL
docker compose up -d

# Stop PostgreSQL
docker compose down

# Run the API
dotnet run --project src/AiGroupChat.API

# Build solution
dotnet build

# Add migration
dotnet ef migrations add <Name> --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Apply migrations
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Access PostgreSQL CLI
docker exec -it aigroupchat-db psql -U aigroupchat -d aigroupchat_dev
```

---

## Configuration Reference

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=aigroupchat_dev;Username=aigroupchat;Password=devpassword123"
  },
  "Email": {
    "ApiKey": "re_your_resend_api_key_here",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "AI Group Chat",
    "FrontendBaseUrl": "http://localhost:3000",
    "ConfirmEmailPath": "/confirm-email",
    "ResetPasswordPath": "/reset-password"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters",
    "Issuer": "AiGroupChat",
    "Audience": "AiGroupChat",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## Environment Setup (For New Sessions)

To continue development in a new session:

1. Navigate to project: `cd ~/Projects/AiGroupChat`
2. Start database: `docker compose up -d`
3. Verify database: `docker ps` (should show aigroupchat-db)
4. Pull latest: `git pull origin main`
5. Build: `dotnet build`
6. Run: `dotnet run --project src/AiGroupChat.API`
7. Access Scalar docs: `http://localhost:<port>/scalar/v1`

---

## Notes & Decisions Made

### Session 1
1. **Port 5434** for PostgreSQL (5433 was in use by another project)
2. **Snake_case** for database table/column names (PostgreSQL convention)
3. **Scalar** chosen over Swagger for API documentation (cleaner UI)
4. **Secrets in appsettings.Development.json** (gitignored) for local dev
5. **User.Id is string** (ASP.NET Identity default) while other entities use Guid
6. **AI Service is separate** - Python FastAPI project, not part of this solution

### Session 2
7. **Separate Email project** - Provider-agnostic for easy swapping (Resend → Mailgun)
8. **HTML templates as embedded resources** - Clean separation, easy to edit
9. **Frontend-first email flow** - Confirmation links go to frontend, not API
10. **Option A architecture for auth** - Split services (AuthService in Application, TokenService in Infrastructure)
11. **Token expiration** - 15 min access, 7 day refresh, 24 hour email confirm, 1 hour password reset
