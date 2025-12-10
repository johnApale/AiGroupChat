# Auth Controller Integration Tests

This directory contains integration tests for all authentication endpoints in the AI Group Chat API.

## Test Coverage

| Test File                    | Endpoint                             | Tests | Description                      |
| ---------------------------- | ------------------------------------ | ----- | -------------------------------- |
| `RegisterTests.cs`           | `POST /api/auth/register`            | 6     | User registration and validation |
| `LoginTests.cs`              | `POST /api/auth/login`               | 6     | User authentication              |
| `ConfirmEmailTests.cs`       | `POST /api/auth/confirm-email`       | 5     | Email verification               |
| `ResendConfirmationTests.cs` | `POST /api/auth/resend-confirmation` | 5     | Resend confirmation email        |
| `ForgotPasswordTests.cs`     | `POST /api/auth/forgot-password`     | 5     | Password reset request           |
| `ResetPasswordTests.cs`      | `POST /api/auth/reset-password`      | 6     | Password reset completion        |
| `RefreshTokenTests.cs`       | `POST /api/auth/refresh`             | 6     | Token refresh                    |
| `LogoutTests.cs`             | `POST /api/auth/logout`              | 5     | Session termination              |

**Total: 44 tests**

## Test Details

### RegisterTests.cs

| Test                                             | Expected Result                      |
| ------------------------------------------------ | ------------------------------------ |
| `Register_WithValidData_Returns201AndSendsEmail` | 201 Created, confirmation email sent |
| `Register_WithDuplicateEmail_Returns400`         | 400 Bad Request                      |
| `Register_WithDuplicateUsername_Returns400`      | 400 Bad Request                      |
| `Register_WithInvalidEmail_Returns400`           | 400 Bad Request                      |
| `Register_WithShortUsername_Returns400`          | 400 Bad Request                      |
| `Register_WithWeakPassword_Returns400`           | 400 Bad Request                      |

### LoginTests.cs

| Test                                       | Expected Result                   |
| ------------------------------------------ | --------------------------------- |
| `Login_WithValidCredentials_ReturnsTokens` | 200 OK with access/refresh tokens |
| `Login_WithUnconfirmedEmail_Returns401`    | 401 Unauthorized                  |
| `Login_WithWrongPassword_Returns401`       | 401 Unauthorized                  |
| `Login_WithNonExistentEmail_Returns401`    | 401 Unauthorized                  |
| `Login_WithInvalidEmailFormat_Returns400`  | 400 Bad Request                   |
| `Login_WithEmptyPassword_Returns400`       | 400 Bad Request                   |

### ConfirmEmailTests.cs

| Test                                           | Expected Result                   |
| ---------------------------------------------- | --------------------------------- |
| `ConfirmEmail_WithValidToken_ReturnsTokens`    | 200 OK with access/refresh tokens |
| `ConfirmEmail_WithInvalidToken_Returns400`     | 400 Bad Request                   |
| `ConfirmEmail_WithNonExistentEmail_Returns400` | 400 Bad Request (security)        |
| `ConfirmEmail_WhenAlreadyConfirmed_ReturnsOk`  | 200 OK (idempotent)               |
| `ConfirmEmail_WithMissingToken_Returns400`     | 400 Bad Request                   |

### ResendConfirmationTests.cs

| Test                                                      | Expected Result                         |
| --------------------------------------------------------- | --------------------------------------- |
| `ResendConfirmation_WithUnconfirmedEmail_SendsEmail`      | 200 OK, email sent                      |
| `ResendConfirmation_WithConfirmedEmail_Returns200NoEmail` | 200 OK, no email (prevents enumeration) |
| `ResendConfirmation_WithNonExistentEmail_Returns200`      | 200 OK (prevents enumeration)           |
| `ResendConfirmation_WithInvalidEmailFormat_Returns400`    | 400 Bad Request                         |
| `ResendConfirmation_NewTokenWorksForConfirmation`         | New token can confirm email             |

### ForgotPasswordTests.cs

| Test                                                          | Expected Result               |
| ------------------------------------------------------------- | ----------------------------- |
| `ForgotPassword_WithExistingEmail_SendsResetEmail`            | 200 OK, reset email sent      |
| `ForgotPassword_WithNonExistentEmail_Returns200NoEmail`       | 200 OK (prevents enumeration) |
| `ForgotPassword_WithUnconfirmedEmail_Returns200AndSendsEmail` | 200 OK, email sent            |
| `ForgotPassword_WithInvalidEmailFormat_Returns400`            | 400 Bad Request               |
| `ForgotPassword_WithEmptyEmail_Returns400`                    | 400 Bad Request               |

### ResetPasswordTests.cs

| Test                                            | Expected Result                   |
| ----------------------------------------------- | --------------------------------- |
| `ResetPassword_WithValidToken_ResetsPassword`   | 200 OK, password changed          |
| `ResetPassword_OldPasswordNoLongerWorks`        | Old password rejected after reset |
| `ResetPassword_WithInvalidToken_Returns400`     | 400 Bad Request                   |
| `ResetPassword_WithNonExistentEmail_Returns400` | 400 Bad Request (security)        |
| `ResetPassword_WithWeakPassword_Returns400`     | 400 Bad Request                   |
| `ResetPassword_TokenCanOnlyBeUsedOnce`          | Second use returns 400            |

### RefreshTokenTests.cs

| Test                                           | Expected Result                |
| ---------------------------------------------- | ------------------------------ |
| `RefreshToken_WithValidToken_ReturnsNewTokens` | 200 OK with new tokens         |
| `RefreshToken_WithInvalidToken_Returns401`     | 401 Unauthorized               |
| `RefreshToken_WithEmptyToken_Returns400`       | 400 Bad Request                |
| `RefreshToken_OldTokenInvalidatedAfterRefresh` | Old token rejected             |
| `RefreshToken_NewTokenCanBeUsed`               | New refresh token works        |
| `RefreshToken_NewAccessTokenWorksForAuth`      | New access token authenticates |

### LogoutTests.cs

| Test                                        | Expected Result               |
| ------------------------------------------- | ----------------------------- |
| `Logout_WithValidRefreshToken_Returns200`   | 200 OK                        |
| `Logout_RefreshTokenInvalidatedAfterLogout` | Token rejected after logout   |
| `Logout_WithInvalidRefreshToken_Returns200` | 200 OK (prevents enumeration) |
| `Logout_WithEmptyRefreshToken_Returns400`   | 400 Bad Request               |
| `Logout_CanLoginAgainAfterLogout`           | New login succeeds            |

## Security Considerations

Several tests verify security best practices:

1. **User Enumeration Prevention**: Endpoints like `forgot-password`, `resend-confirmation`, and `reset-password` return generic responses (200 OK or 400 Bad Request) regardless of whether the email exists. This prevents attackers from discovering valid email addresses.

2. **Token Invalidation**: Tests verify that refresh tokens are properly invalidated after:

   - Logout
   - Token refresh (rotation)
   - Password reset

3. **Idempotent Operations**: Email confirmation is idempotent - confirming an already-confirmed email returns success rather than an error.

## Running Tests

```bash
# Run all auth tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Auth"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "RegisterTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "LoginTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "LogoutTests"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Auth" -v n
```

## Test Infrastructure

These tests use:

- **Testcontainers**: Spins up a real PostgreSQL database in Docker for each test run
- **FakeEmailProvider**: Captures emails in memory for assertion
- **AuthHelper**: Provides reusable methods for common auth operations
- **DatabaseCleaner**: Cleans tables between tests for isolation

See the main [Integration Tests README](../../README.md) for full infrastructure documentation.
