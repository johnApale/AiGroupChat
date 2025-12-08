# AI Group Chat - Copilot Instructions

## Project Overview

A group chat application with integrated AI agent capabilities built with ASP.NET Core 9 and PostgreSQL. Users can create groups, invite members, and interact with AI agents using multiple provider backends (Gemini, Claude, OpenAI, Grok). The AI service is architecturally separate—a stateless Python FastAPI service (separate repo) that the .NET app calls when AI responses are needed.

## Agent Workflow Rules

### Task Execution

- **Incremental changes** - Don't implement everything at once. Complete one piece, verify it works, then move to the next.
- **Step-by-step approach** - Break features into logical steps (e.g., 1. Domain entity → 2. Repository interface → 3. Repository implementation → 4. Service → 5. Controller → 6. Tests)
- **Ask before assuming** - If requirements are unclear or multiple approaches exist, ask for clarification before writing code.

### Context & Consistency

- **Reference the spec file** - Before implementing any feature, consult `/docs/spec/ai-group-chat-spec.md` for:
  - Feature requirements and acceptance criteria
  - API endpoint contracts (routes, request/response shapes)
  - Business rules and edge cases
  - Expected behavior and validation rules
- **Reference existing code** - Before creating new files, examine similar implementations in the project. Match existing patterns exactly.
- **Follow established conventions** - Adhere to Clean Architecture layers, naming conventions, and patterns documented in this file.

### Git & Documentation

- **Commit only complete features** - Don't commit partial work. A feature is complete when all layers (domain, application, infrastructure, API, tests) are implemented and working.
- **Update README.md** - After completing a feature, document any new endpoints, configuration changes, environment variables, or setup steps.

**Key Architecture Decision**: The Python AI service is intentionally stateless with no database, designed to be sold as a standalone product. The .NET app passes all context with each request.

## Clean Architecture Layers

The codebase follows strict Clean Architecture with distinct dependency rules:

```
API → Infrastructure → Application → Domain
```

- **Domain** (`AiGroupChat.Domain/`) - Pure entities, enums, interfaces. Zero external dependencies.
- **Application** (`AiGroupChat.Application/`) - Business logic, DTOs, service interfaces. Depends only on Domain.
- **Infrastructure** (`AiGroupChat.Infrastructure/`) - EF Core, external APIs, service implementations. Depends on Application.
- **API** (`AiGroupChat.API/`) - Controllers, middleware, SignalR hubs (planned). Entry point that wires everything together.

**Critical**: Never reference Infrastructure from Application. Application defines interfaces (e.g., `IUserRepository`), Infrastructure implements them (`IdentityUserRepository`).

## Dependency Injection Pattern

Each layer registers its own services via a `DependencyInjection.cs` file with an extension method:

```csharp
// Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)

// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)

// Email/DependencyInjection.cs
public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
```

Chain these in `Program.cs`: `builder.Services.AddInfrastructure(builder.Configuration)` which internally calls `AddApplication()` and `AddEmail()`.

## Entity Framework Core Conventions

### DbContext Setup

- `ApplicationDbContext` inherits from `IdentityDbContext<User>` (not plain `DbContext`)
- Uses Fluent API configurations in `Data/Configurations/` directory
- All configurations implement `IEntityTypeConfiguration<TEntity>`
- Configurations auto-discovered via `builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)`

### Entity Configuration Patterns

- Table names: lowercase with underscores (`groups`, `group_members`)
- Column names: snake_case (`created_at`, `ai_monitoring_enabled`, `created_by`)
- All entities have `CreatedAt` and `UpdatedAt` DateTime properties
- Use `HasMaxLength()` for string properties to avoid unbounded columns

Example from `GroupConfiguration.cs`:

```csharp
builder.Property(x => x.Name)
    .HasColumnName("name")
    .HasMaxLength(200)
    .IsRequired();
```

### Migrations

Run migrations from solution root:

```bash
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API
```

Create new migration:

```bash
dotnet ef migrations add MigrationName --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API
```

## Authentication & Authorization

- Uses ASP.NET Identity with custom `User` entity extending `IdentityUser`
- JWT tokens with 15-minute expiry, 7-day refresh tokens
- Email confirmation required before login (`options.SignIn.RequireConfirmedEmail = true`)
- All auth logic in `AuthService`, no controllers manipulating `UserManager` directly

### Repository Pattern for Identity

The app wraps `UserManager<User>` with `IUserRepository` interface (implemented by `IdentityUserRepository`) to maintain clean architecture boundaries. Services depend on `IUserRepository`, not directly on Identity.

## Exception Handling Architecture

Custom exceptions in `Application/Exceptions/`:

- `AuthenticationException` → 401
- `AuthorizationException` → 403
- `NotFoundException` → 404
- `ValidationException` → 400 (can include error array)

Global exception handling via `ExceptionHandlingMiddleware` (registered first in pipeline). Never throw raw exceptions—use custom types for predictable API responses.

## DTO Conventions

DTOs organized by feature in `Application/DTOs/{Feature}/`:

- Request DTOs: `LoginRequest`, `CreateGroupRequest`
- Response DTOs: `AuthResponse`, `GroupResponse`
- Shared DTOs: `MessageResponse` (generic success message)

Use `DataAnnotations` for validation (`[Required]`, `[EmailAddress]`, `[MaxLength]`). Validation errors automatically converted to `ValidationException` by framework.

## Email Service

Uses Resend API via `AiGroupChat.Email` project. Configuration in `appsettings.json`:

```json
"Email": {
  "ApiKey": "",
  "FromEmail": "",
  "FromName": "AI Group Chat",
  "FrontendBaseUrl": "",
  "ConfirmEmailPath": "/confirm-email",
  "ResetPasswordPath": "/reset-password"
}
```

Email templates use Razor views in `Email/Templates/`. The service constructs full URLs by combining `FrontendBaseUrl` with path and token parameters.

## Testing Conventions

Tests follow AAA pattern (Arrange-Act-Assert) with descriptive naming: `MethodCondition_ExpectedResult`

Example: `WithValidCredentials_ReturnsAuthResponse`, `WithInvalidEmail_ThrowsAuthenticationException`

### Test Structure

Each service has a base class with shared setup:

- `AuthServiceTestBase` provides mocked dependencies (`UserRepositoryMock`, `TokenServiceMock`, etc.)
- Individual test files inherit the base (e.g., `LoginAsyncTests : AuthServiceTestBase`)
- README.md in each test folder documents coverage and patterns

Run specific test suites:

```bash
dotnet test --filter "FullyQualifiedName~AuthService"
dotnet test --filter "FullyQualifiedName~LoginAsyncTests"
```

## Development Workflow

### First-Time Setup

```bash
# Start PostgreSQL (port 5434 to avoid conflicts)
docker compose up -d

# Apply migrations
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Run the API
dotnet run --project src/AiGroupChat.API
```

### Connection String

Default dev connection in `docker-compose.yml`:

```
Host=localhost;Port=5434;Database=aigroupchat_dev;Username=aigroupchat;Password=devpassword123
```

### API Documentation

Uses Scalar (OpenAPI viewer) available at `/scalar/v1` when running in Development mode.

## Key Domain Entities

- **User** - Extends `IdentityUser`, has `DisplayName`, tracks `CreatedGroups` and `GroupMemberships`
- **Group** - Has owner (`CreatedById`), `AiMonitoringEnabled` toggle, optional `AiProviderId`
- **GroupMember** - Join table with `Role` enum (Owner/Admin/Member)
- **Message** - Sent by User or AI, links to Group, tracks AI metadata
- **AiProvider** - Configuration for AI backends (Gemini, Claude, etc.)
- **RefreshToken** - Stores refresh tokens with expiry and revocation tracking

## Configuration Settings

JWT settings in `appsettings.json`:

```json
"Jwt": {
  "Secret": "",
  "Issuer": "AiGroupChat",
  "Audience": "AiGroupChat",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

Bind configuration objects using the Options pattern with `SectionName` constants (see `JwtSettings.cs`, `EmailSettings.cs`).

## Common Patterns

### Service Layer Pattern

Services implement interfaces defined in `Application/Interfaces/`. Pattern:

1. Interface in Application layer (`IAuthService`)
2. Implementation in Application layer (`AuthService`)
3. Dependencies injected via constructor (repositories, other services)
4. All async methods accept optional `CancellationToken`

### Repository Layer Pattern

Repositories abstract data access:

1. Interface in Application layer (`IUserRepository`)
2. Implementation in Infrastructure layer (`IdentityUserRepository`)
3. Methods return domain entities, not EF entities
4. Include cancellation token support even when underlying libs don't use it (future-proofing)

## Future Considerations

- **SignalR** planned for real-time messaging (currently REST-only)
- **AI Service Integration** via HTTP client calling Python FastAPI service
- **AI Monitoring** - Only messages sent while `AiMonitoringEnabled=true` visible to AI
- **Multi-tenant** - Groups are isolated; members only see their groups' messages
