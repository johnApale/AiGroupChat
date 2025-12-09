# Integration Tests

Integration tests for the AI Group Chat API. These tests run against a real PostgreSQL database using Testcontainers and verify that endpoints work correctly end-to-end.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) - Must be running for tests to execute
- .NET 9 SDK

## Running Tests

```bash
# Run all integration tests
dotnet test tests/AiGroupChat.IntegrationTests

# Run specific test class
dotnet test tests/AiGroupChat.IntegrationTests --filter "RegisterTests"

# Run with detailed output
dotnet test tests/AiGroupChat.IntegrationTests --logger "console;verbosity=detailed"
```

## Project Structure

```
AiGroupChat.IntegrationTests/
├── Infrastructure/           # Test setup and utilities
│   ├── CustomWebApplicationFactory.cs   # Configures test server + database
│   ├── DatabaseCleaner.cs               # Cleans tables between tests
│   ├── FakeEmailProvider.cs             # Captures emails in memory
│   └── IntegrationTestBase.cs           # Base class for all tests
├── Helpers/                  # Reusable test operations
│   └── AuthHelper.cs                    # Register, login, confirm helpers
└── Controllers/              # Tests organized by controller
    └── Auth/
        └── RegisterTests.cs
```

## Architecture

### CustomWebApplicationFactory

Extends `WebApplicationFactory<Program>` to:

- Spin up a fresh PostgreSQL container via Testcontainers
- Apply database migrations automatically
- Inject test configuration (JWT secret, email settings)
- Replace `IEmailProvider` with `FakeEmailProvider`

### Test Isolation

Each test runs in isolation:

- **Database**: `DatabaseCleaner` deletes all data after each test
- **Email**: `FakeEmailProvider` clears captured emails after each test
- **HTTP Client**: Fresh client per test class

### Helpers

Helpers provide reusable operations for common test scenarios:

- `AuthHelper` - User registration, login, email confirmation
- More helpers added as needed (GroupHelper, MessageHelper, etc.)

## Writing New Tests

### 1. Create a test class

```csharp
using System.Net;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Auth;

public class LoginTests : IntegrationTestBase
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange - Create a confirmed user
        await Auth.RegisterAndConfirmAsync(
            email: "user@example.com",
            password: "TestPass123!");

        // Act
        AuthResponse response = await Auth.LoginAsync("user@example.com", "TestPass123!");

        // Assert
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);
    }
}
```

### 2. Use helpers for common operations

```csharp
// Create an authenticated user (registers, confirms, sets auth header)
AuthResponse user = await Auth.CreateAuthenticatedUserAsync();

// Access the HTTP client directly
HttpResponseMessage response = await Client.GetAsync("/api/groups");

// Check captured emails
Assert.Single(EmailProvider.SentEmails);
string? token = EmailProvider.ExtractTokenFromLastEmail();
```

### 3. Test both success and failure cases

```csharp
[Fact]
public async Task Endpoint_WithValidData_ReturnsSuccess()
{
    // Happy path
}

[Fact]
public async Task Endpoint_WithInvalidData_Returns400()
{
    // Validation errors
}

[Fact]
public async Task Endpoint_WithoutAuth_Returns401()
{
    // Missing authentication
}
```

## Test Configuration

Tests use in-memory configuration defined in `CustomWebApplicationFactory`:

| Setting             | Test Value                                       |
| ------------------- | ------------------------------------------------ |
| JWT Secret          | `ThisIsATestSecretKeyThatIsAtLeast32Characters!` |
| JWT Issuer/Audience | `AiGroupChat`                                    |
| Database            | Fresh PostgreSQL container per test run          |
| Email               | `FakeEmailProvider` (no real emails sent)        |

## Troubleshooting

### Tests fail with "Cannot connect to Docker"

Make sure Docker Desktop is running.

### Tests fail with "key length is zero"

The JWT configuration may not be loading. Ensure `Program.cs` uses lazy configuration:

```csharp
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) => { ... });
```

### Tests are slow

Testcontainers reuses containers when possible. First run downloads the PostgreSQL image (~200MB), subsequent runs are faster.

## Coverage

### Auth Controller

| Endpoint                           | Tests                      |
| ---------------------------------- | -------------------------- |
| POST /api/auth/register            | ✅ RegisterTests (6 tests) |
| POST /api/auth/login               | 🔲 Planned                 |
| POST /api/auth/confirm-email       | 🔲 Planned                 |
| POST /api/auth/resend-confirmation | 🔲 Planned                 |
| POST /api/auth/forgot-password     | 🔲 Planned                 |
| POST /api/auth/reset-password      | 🔲 Planned                 |
| POST /api/auth/refresh             | 🔲 Planned                 |
| POST /api/auth/logout              | 🔲 Planned                 |

### Other Controllers

| Controller   | Status     |
| ------------ | ---------- |
| Groups       | 🔲 Planned |
| GroupMembers | 🔲 Planned |
| Messages     | 🔲 Planned |
| Users        | 🔲 Planned |
| AiProviders  | 🔲 Planned |
