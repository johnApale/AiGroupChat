# Users Controller Integration Tests

## Overview

Integration tests for the `UsersController` endpoints that handle user profile retrieval.

## Endpoints Tested

| Method | Endpoint          | Description                    |
| ------ | ----------------- | ------------------------------ |
| GET    | `/api/users/me`   | Get current authenticated user |
| GET    | `/api/users/{id}` | Get user by ID                 |

## Test Coverage

### UsersControllerTests.cs (6 tests)

| Test                                            | Status Code | Description                                    |
| ----------------------------------------------- | ----------- | ---------------------------------------------- |
| `GetCurrentUser_WithValidToken_ReturnsUserInfo` | 200         | Authenticated user retrieves their own profile |
| `GetCurrentUser_WithoutToken_Returns401`        | 401         | Unauthenticated request is rejected            |
| `GetCurrentUser_WithInvalidToken_Returns401`    | 401         | Invalid/malformed JWT is rejected              |
| `GetById_WithValidId_ReturnsUserInfo`           | 200         | Authenticated user can fetch any user by ID    |
| `GetById_WithNonExistentId_Returns404`          | 404         | Non-existent user ID returns not found         |
| `GetById_WithoutToken_Returns401`               | 401         | Unauthenticated request is rejected            |

## Running the Tests

```bash
# Run all Users controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Users"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Users" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- The `GET /api/users/{id}` endpoint allows any authenticated user to fetch any other user's public profile
- User responses include: `Id`, `Email`, `UserName`, `DisplayName`, `CreatedAt`
