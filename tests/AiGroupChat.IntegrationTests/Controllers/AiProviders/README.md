# AI Providers Controller Integration Tests

## Overview

Integration tests for the `AiProvidersController` endpoints that handle listing and retrieving AI provider configurations.

## Endpoints Tested

| Method | Endpoint                 | Description                |
| ------ | ------------------------ | -------------------------- |
| GET    | `/api/ai-providers`      | List all enabled providers |
| GET    | `/api/ai-providers/{id}` | Get provider by ID         |

## Test Coverage

### GetAllProvidersTests.cs (3 tests)

| Test                              | Status Code | Description                     |
| --------------------------------- | ----------- | ------------------------------- |
| `GetAll_ReturnsProviders`         | 200         | Returns list of providers       |
| `GetAll_ReturnsExpectedProviders` | 200         | Verifies seeded providers exist |
| `GetAll_WithoutToken_Returns401`  | 401         | Auth required                   |

### GetProviderByIdTests.cs (3 tests)

| Test                                   | Status Code | Description              |
| -------------------------------------- | ----------- | ------------------------ |
| `GetById_WithValidId_ReturnsProvider`  | 200         | Returns provider details |
| `GetById_WithNonExistentId_Returns404` | 404         | Provider not found       |
| `GetById_WithoutToken_Returns401`      | 401         | Auth required            |

## Authorization Rules

| Role            | Can List | Can View |
| --------------- | -------- | -------- |
| Authenticated   | ✅       | ✅       |
| Unauthenticated | ❌       | ❌       |

## Provider Response Structure

```json
{
  "id": "guid",
  "name": "gemini",
  "displayName": "Google Gemini",
  "defaultModel": "gemini-1.5-pro",
  "defaultTemperature": 0.7,
  "maxTokensLimit": 1000000
}
```

## Running the Tests

```bash
# Run all AiProviders controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.AiProviders"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetAllProvidersTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetProviderByIdTests"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.AiProviders" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- Only enabled providers are returned (disabled providers are hidden)
- Providers are seeded during database migration
- The MVP includes Gemini as the default provider
