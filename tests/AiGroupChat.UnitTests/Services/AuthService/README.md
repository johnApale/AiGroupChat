# AuthService Unit Tests

Unit tests for the `AuthService` class which handles all authentication business logic.

## Structure

```
AuthService/
├── AuthServiceTestBase.cs           # Shared test setup and mocks
├── RegisterAsyncTests.cs            # User registration tests (regular and invite-based)
├── LoginAsyncTests.cs               # User login tests
├── ConfirmEmailAsyncTests.cs        # Email confirmation tests
├── ResendConfirmationAsyncTests.cs  # Resend confirmation tests
├── ForgotPasswordAsyncTests.cs      # Password reset request tests
├── ResetPasswordAsyncTests.cs       # Password reset tests
├── RefreshTokenAsyncTests.cs        # Token refresh tests
├── LogoutAsyncTests.cs              # Logout tests
└── README.md                        # This file
```

## Test Base Class

`AuthServiceTestBase` provides shared setup for all test classes:

- `UserRepositoryMock` - Mocked `IUserRepository`
- `TokenServiceMock` - Mocked `ITokenService`
- `EmailServiceMock` - Mocked `IEmailService`
- `InvitationRepositoryMock` - Mocked `IGroupInvitationRepository`
- `GroupRepositoryMock` - Mocked `IGroupRepository`
- `AuthService` - Instance under test with mocked dependencies

All test classes inherit from this base class.

## Test Coverage

| File                              | Tests | Scenarios Covered                                                               |
| --------------------------------- | ----- | ------------------------------------------------------------------------------- |
| `RegisterAsyncTests.cs`           | 9     | Valid registration, validation errors, email sending, invite-based registration |
| `LoginAsyncTests.cs`              | 4     | Valid login, invalid email, unconfirmed email, wrong password                   |
| `ConfirmEmailAsyncTests.cs`       | 3     | Valid token, nonexistent email, invalid token                                   |
| `ResendConfirmationAsyncTests.cs` | 3     | Unconfirmed user, already confirmed, nonexistent email                          |
| `ForgotPasswordAsyncTests.cs`     | 2     | Existing user, nonexistent email (enumeration prevention)                       |
| `ResetPasswordAsyncTests.cs`      | 4     | Valid reset, nonexistent email, invalid token, weak password                    |
| `RefreshTokenAsyncTests.cs`       | 4     | Valid refresh, old token revocation, invalid token, deleted user                |
| `LogoutAsyncTests.cs`             | 2     | Valid logout, nonexistent token                                                 |

**Total: 31 tests**

## Invite-Based Registration Tests

The `RegisterAsyncTests.cs` file includes tests for the invite-based registration flow:

| Test                                                   | Description                                           |
| ------------------------------------------------------ | ----------------------------------------------------- |
| `WithValidInviteToken_CreatesUserAndAddsToGroup`       | Valid token creates user, adds to group, returns auth |
| `WithValidInviteToken_ConfirmsEmailDirectly`           | Email is auto-confirmed, no confirmation email sent   |
| `WithValidInviteToken_MarksInvitationAsAccepted`       | Invitation status updated to Accepted                 |
| `WithInvalidInviteToken_ThrowsValidationException`     | Invalid token rejected before user creation           |
| `WithExpiredInviteToken_ThrowsValidationException`     | Expired token rejected                                |
| `WithAlreadyUsedInviteToken_ThrowsValidationException` | Already accepted token rejected                       |
| `WithEmailMismatch_ThrowsValidationException`          | Registration email must match invitation email        |

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run only AuthService tests
dotnet test --filter "FullyQualifiedName~AuthService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~LoginAsyncTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~LoginAsyncTests.WithValidCredentials_ReturnsAuthResponse"
```

## Test Patterns

### Naming Convention

Tests follow the pattern: `MethodCondition_ExpectedResult`

Examples:

- `WithValidCredentials_ReturnsAuthResponse`
- `WithInvalidEmail_ThrowsAuthenticationException`
- `WithNonexistentEmail_ReturnsSuccessToPreventEnumeration`

### Arrange-Act-Assert

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task WithValidCredentials_ReturnsAuthResponse()
{
    // Arrange - Set up mocks and test data
    LoginRequest request = new LoginRequest { ... };
    UserRepositoryMock.Setup(...);

    // Act - Call the method under test
    AuthResponse result = await AuthService.LoginAsync(request);

    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal("expected", result.Value);
}
```

### Verifying Side Effects

Some tests verify that certain methods were called:

```csharp
// Verify email was sent
EmailServiceMock.Verify(
    x => x.SendConfirmationEmailAsync(...),
    Times.Once);

// Verify token was revoked
TokenServiceMock.Verify(
    x => x.RevokeRefreshTokenAsync(...),
    Times.Once);
```

## Security Test Cases

Several tests verify security-related behavior:

| Scenario                                 | Expected Behavior                            |
| ---------------------------------------- | -------------------------------------------- |
| Nonexistent email on forgot password     | Returns success (prevents email enumeration) |
| Nonexistent email on resend confirmation | Returns success (prevents email enumeration) |
| Password reset                           | Revokes all refresh tokens                   |
| Token refresh                            | Revokes old refresh token                    |
| Unconfirmed email login                  | Throws `AuthenticationException`             |

## Adding New Tests

1. Identify which test file the new test belongs to
2. Add a new `[Fact]` method following the naming convention
3. Use the mocks from `AuthServiceTestBase`
4. Follow the Arrange-Act-Assert pattern
5. Run tests to verify: `dotnet test`

Example:

```csharp
[Fact]
public async Task WithSpecificCondition_ExpectedBehavior()
{
    // Arrange
    SomeRequest request = new SomeRequest { ... };

    UserRepositoryMock
        .Setup(x => x.SomeMethod(...))
        .ReturnsAsync(expectedValue);

    // Act
    SomeResponse result = await AuthService.SomeMethodAsync(request);

    // Assert
    Assert.NotNull(result);
}
```
