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

# Run all SignalR tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Hubs.ChatHub"

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
│   ├── IntegrationTestBase.cs           # Base class for REST API tests
│   ├── SignalRCollection.cs             # xUnit collection for SignalR tests
│   └── SignalRIntegrationTestBase.cs    # Base class for SignalR tests
├── Helpers/                  # Reusable test operations
│   ├── AuthHelper.cs                    # Register, login, confirm helpers
│   ├── GroupHelper.cs                   # Group CRUD helpers
│   ├── GroupMemberHelper.cs             # Member management helpers
│   ├── MessageHelper.cs                 # Message send/retrieve helpers
│   ├── AiProviderHelper.cs              # AI provider listing helpers
│   └── SignalRHelper.cs                 # SignalR connection + event helpers
├── Controllers/              # REST API tests organized by controller
│   ├── Auth/                            # Auth endpoint tests
│   ├── Users/                           # User endpoint tests
│   ├── Groups/                          # Group CRUD tests
│   ├── GroupMembers/                    # Member management tests
│   ├── GroupOwner/                      # Ownership transfer tests
│   ├── Messages/                        # Message send/retrieve tests
│   └── AiProviders/                     # AI provider listing tests
└── Hubs/                     # SignalR hub tests
    └── ChatHub/                         # ChatHub integration tests
        ├── ConnectionTests.cs           # WebSocket connection/auth
        ├── JoinLeaveGroupTests.cs       # SignalR group subscription
        ├── TypingIndicatorTests.cs      # Typing broadcasts
        ├── MessageBroadcastTests.cs     # Message delivery via WebSocket
        ├── MemberEventTests.cs          # Member join/leave/role events
        ├── AiSettingsEventTests.cs      # AI settings broadcasts
        └── PresenceTests.cs             # Online/offline presence
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
- **SignalR**: Connections auto-disposed after each test

### Helpers

Helpers provide reusable operations for common test scenarios:

- `AuthHelper` - User registration, login, email confirmation
- `GroupHelper` - Group creation, update, delete, AI settings
- `GroupMemberHelper` - Add/remove members, update roles, leave group, transfer ownership
- `MessageHelper` - Send and retrieve messages
- `AiProviderHelper` - List and retrieve AI providers
- `SignalRHelper` - WebSocket connection, event collection, hub method invocation

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

### 2. Create a SignalR test class

```csharp
using AiGroupChat.IntegrationTests.Infrastructure;
using AiGroupChat.IntegrationTests.Helpers;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

[Collection("SignalR")]
public class MySignalRTests : SignalRIntegrationTestBase
{
    public MySignalRTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task MyTest_Scenario_ExpectedResult()
    {
        // Arrange - Create user and connect
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync();
        SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);

        // Act - Join group and wait for events
        await connection.JoinGroupAsync(groupId);

        // Assert - Wait for expected event
        MessageResponse msg = await connection.WaitForMessageAsync(m => m.Content == "Hello");
        Assert.NotNull(msg);
    }
}
```

### 3. Use helpers for common operations

```csharp
// Create an authenticated user (registers, confirms, sets auth header)
AuthResponse user = await Auth.CreateAuthenticatedUserAsync();

// Access the HTTP client directly
HttpResponseMessage response = await Client.GetAsync("/api/groups");

// Check captured emails
Assert.Single(EmailProvider.SentEmails);
string? token = EmailProvider.ExtractTokenFromLastEmail();

// Create SignalR connection and wait for events
SignalRHelper connection = await CreateSignalRConnectionAsync(user.AccessToken);
await connection.JoinGroupAsync(groupId);
MessageResponse msg = await connection.WaitForMessageAsync(m => m.GroupId == groupId);
```

### 4. Test both success and failure cases

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

## SignalR Test Configuration

SignalR tests use sequential execution via `[Collection("SignalR")]` to avoid:

- ConnectionTracker singleton conflicts
- Database race conditions
- Flaky parallel WebSocket tests

Default timeout for waiting on SignalR events: **5 seconds**

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

### SignalR tests timeout

- Ensure Docker is running (database needed)
- Check if the event is actually being broadcast by the service
- Verify the correct SignalR group was joined before expecting events

## Coverage

### Auth Controller (6 tests)

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

### Users Controller (6 tests)

| Endpoint            | Tests                             |
| ------------------- | --------------------------------- |
| GET /api/users/me   | ✅ UsersControllerTests (3 tests) |
| GET /api/users/{id} | ✅ UsersControllerTests (3 tests) |

### Groups Controller (25 tests)

| Endpoint                | Tests                              |
| ----------------------- | ---------------------------------- |
| POST /api/groups        | ✅ CreateGroupTests (4 tests)      |
| GET /api/groups         | ✅ GetMyGroupsTests (3 tests)      |
| GET /api/groups/{id}    | ✅ GetGroupByIdTests (4 tests)     |
| PUT /api/groups/{id}    | ✅ UpdateGroupTests (5 tests)      |
| DELETE /api/groups/{id} | ✅ DeleteGroupTests (4 tests)      |
| PUT /api/groups/{id}/ai | ✅ UpdateAiSettingsTests (5 tests) |

### GroupMembers Controller (38 tests)

| Endpoint                                   | Tests                               |
| ------------------------------------------ | ----------------------------------- |
| POST /api/groups/{id}/members              | ✅ AddMemberTests (8 tests)         |
| GET /api/groups/{id}/members               | ✅ GetMembersTests (4 tests)        |
| PUT /api/groups/{id}/members/{memberId}    | ✅ UpdateMemberRoleTests (10 tests) |
| DELETE /api/groups/{id}/members/{memberId} | ✅ RemoveMemberTests (10 tests)     |
| DELETE /api/groups/{id}/members/me         | ✅ LeaveGroupTests (6 tests)        |

### GroupOwner Controller (9 tests)

| Endpoint                   | Tests                               |
| -------------------------- | ----------------------------------- |
| PUT /api/groups/{id}/owner | ✅ TransferOwnershipTests (9 tests) |

### Messages Controller (16 tests)

| Endpoint                       | Tests                         |
| ------------------------------ | ----------------------------- |
| POST /api/groups/{id}/messages | ✅ SendMessageTests (7 tests) |
| GET /api/groups/{id}/messages  | ✅ GetMessagesTests (9 tests) |

### AiProviders Controller (6 tests)

| Endpoint                   | Tests                             |
| -------------------------- | --------------------------------- |
| GET /api/ai-providers      | ✅ GetAllProvidersTests (3 tests) |
| GET /api/ai-providers/{id} | ✅ GetProviderByIdTests (3 tests) |

### ChatHub SignalR (40 tests)

| Feature              | Tests                              |
| -------------------- | ---------------------------------- |
| Connection/Auth      | ✅ ConnectionTests (3 tests)       |
| Join/Leave Groups    | ✅ JoinLeaveGroupTests (6 tests)   |
| Typing Indicators    | ✅ TypingIndicatorTests (5 tests)  |
| Message Broadcasting | ✅ MessageBroadcastTests (7 tests) |
| Member Events        | ✅ MemberEventTests (8 tests)      |
| AI Settings Events   | ✅ AiSettingsEventTests (4 tests)  |
| Presence (Online)    | ✅ PresenceTests (7 tests)         |

### Summary

| Category     | Tests   |
| ------------ | ------- |
| Auth         | 6       |
| Users        | 6       |
| Groups       | 25      |
| GroupMembers | 38      |
| GroupOwner   | 9       |
| Messages     | 16      |
| AiProviders  | 6       |
| **SignalR**  | **40**  |
| **Total**    | **146** |
