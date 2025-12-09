# Session 12: Integration Tests Setup

## Overview

Set up the integration testing infrastructure for the AI Group Chat API. Integration tests run against a real PostgreSQL database using Testcontainers, allowing us to verify endpoints work correctly end-to-end.

## Goals

- [x] Create integration test project
- [x] Set up Testcontainers for PostgreSQL
- [x] Create test infrastructure (factory, helpers, base class)
- [x] Implement first set of tests (Register endpoint)
- [x] Fix JWT configuration to support test overrides

## Implementation

### 1. Created Integration Test Project

```bash
dotnet new xunit -n AiGroupChat.IntegrationTests -o tests/AiGroupChat.IntegrationTests
dotnet sln add tests/AiGroupChat.IntegrationTests/AiGroupChat.IntegrationTests.csproj
```

Added packages:

- `Microsoft.AspNetCore.Mvc.Testing` - In-memory test server
- `Testcontainers.PostgreSql` - Spins up PostgreSQL in Docker
- `Respawn` - Database cleanup (available for future use)

### 2. Test Infrastructure

Created a modular, clean architecture for tests:

```
AiGroupChat.IntegrationTests/
├── Infrastructure/
│   ├── CustomWebApplicationFactory.cs   # Test server + DB setup
│   ├── DatabaseCleaner.cs               # Table cleanup
│   ├── FakeEmailProvider.cs             # Email capture
│   └── IntegrationTestBase.cs           # Base class
├── Helpers/
│   └── AuthHelper.cs                    # Auth operations
└── Controllers/
    └── Auth/
        └── RegisterTests.cs             # First tests
```

#### CustomWebApplicationFactory

- Starts a PostgreSQL container via Testcontainers
- Runs migrations automatically
- Injects test configuration (JWT, email settings)
- Replaces email provider with fake

#### FakeEmailProvider

- Implements `IEmailProvider`
- Captures sent emails in memory
- Provides `ExtractTokenFromLastEmail()` for confirmation flows

#### DatabaseCleaner

- Static utility to delete all table data
- Respects foreign key order
- Called after each test for isolation

#### IntegrationTestBase

- Thin base class that composes helpers
- Provides `Client`, `EmailProvider`, `Auth` to tests
- Handles cleanup via `IAsyncLifetime`

### 3. Fixed JWT Configuration

**Problem**: JWT settings were read eagerly at startup, before test configuration could be injected.

**Original code**:

```csharp
JwtSettings jwtSettings = builder.Configuration.GetSection(...).Get<JwtSettings>()!;
builder.Services.AddJwtBearer(options =>
{
    options.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
});
```

**Fixed code**:

```csharp
builder.Services.AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        JwtSettings jwtSettings = configuration.GetSection(...).Get<JwtSettings>()!;
        options.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
    });
```

This defers configuration reading until the options are actually needed, allowing `WebApplicationFactory` to inject test values first.

### 4. Register Endpoint Tests

Created 6 tests for `/api/auth/register`:

| Test                                                         | Description                |
| ------------------------------------------------------------ | -------------------------- |
| `Register_WithValidData_Returns201AndSendsConfirmationEmail` | Happy path                 |
| `Register_WithDuplicateEmail_Returns400`                     | Email uniqueness           |
| `Register_WithDuplicateUserName_Returns400`                  | Username uniqueness        |
| `Register_WithInvalidEmail_Returns400`                       | Email format validation    |
| `Register_WithShortUserName_Returns400`                      | Username length validation |
| `Register_WithWeakPassword_Returns400`                       | Password requirements      |

## Key Decisions

### Why Testcontainers over In-Memory Database?

- Tests run against real PostgreSQL, matching production
- Catches SQL-specific issues (constraints, data types)
- EF Core In-Memory provider doesn't support all features

### Why In-Memory Config over appsettings.Testing.json?

- All test config visible in one place
- No extra files to manage
- Easy to see what's different for tests
- Can add file later if config grows

### Why Modular Helper Classes?

- Single responsibility principle
- Easy to add new helpers (GroupHelper, MessageHelper)
- Tests stay focused on assertions, not setup
- Reusable across test classes

## Files Changed

### New Files

- `tests/AiGroupChat.IntegrationTests/AiGroupChat.IntegrationTests.csproj`
- `tests/AiGroupChat.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`
- `tests/AiGroupChat.IntegrationTests/Infrastructure/DatabaseCleaner.cs`
- `tests/AiGroupChat.IntegrationTests/Infrastructure/FakeEmailProvider.cs`
- `tests/AiGroupChat.IntegrationTests/Infrastructure/IntegrationTestBase.cs`
- `tests/AiGroupChat.IntegrationTests/Helpers/AuthHelper.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Auth/RegisterTests.cs`
- `tests/AiGroupChat.IntegrationTests/README.md`

### Modified Files

- `AiGroupChat.sln` - Added integration test project
- `src/AiGroupChat.API/Program.cs` - Added `public partial class Program`, fixed JWT config

## Running Tests

```bash
# Ensure Docker is running, then:
dotnet test tests/AiGroupChat.IntegrationTests --filter "RegisterTests"
```

Output:

```
[testcontainers.org] Docker container ready
info: Applying migration 'InitialCreate'
...
Test summary: total: 6, failed: 0, succeeded: 6, skipped: 0
```

## Next Steps

1. Add remaining Auth tests (Login, ConfirmEmail, Logout, etc.)
2. Add Groups controller tests
3. Add GroupMembers controller tests
4. Add Messages controller tests
5. Consider adding test coverage reporting
