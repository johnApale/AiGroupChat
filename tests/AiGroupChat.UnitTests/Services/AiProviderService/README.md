# AiProviderService Unit Tests

Unit tests for the `AiProviderService` class which handles AI provider listing and retrieval.

## Structure

```
AiProviderService/
├── AiProviderServiceTestBase.cs  # Shared test setup and mocks
├── GetAllAsyncTests.cs           # List providers tests
├── GetByIdAsyncTests.cs          # Get provider by ID tests
└── README.md                     # This file
```

## Test Base Class

`AiProviderServiceTestBase` provides shared setup for all test classes:

- `AiProviderRepositoryMock` - Mocked `IAiProviderRepository`
- `TestProviders` - List of test AI providers (Gemini, Claude, OpenAI)
- `AiProviderService` - Instance under test with mocked dependencies

All test classes inherit from this base class.

## Test Coverage

| File                   | Tests | Scenarios Covered                                      |
| ---------------------- | ----- | ------------------------------------------------------ |
| `GetAllAsyncTests.cs`  | 3     | Returns all providers, empty list, correct DTO mapping |
| `GetByIdAsyncTests.cs` | 3     | Valid ID, nonexistent ID, disabled provider            |

**Total: 6 tests**

## Running Tests

```bash
# Run all tests
dotnet test

# Run only AiProviderService tests
dotnet test --filter "FullyQualifiedName~AiProviderService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~GetAllAsyncTests"
```

## Test Scenarios

### GetAllAsync

1. **WithEnabledProviders_ReturnsAllProviders** - Verifies all enabled providers are returned in order
2. **WithNoProviders_ReturnsEmptyList** - Verifies empty list when no providers exist
3. **ReturnsCorrectDtoMapping** - Verifies entity-to-DTO mapping is correct

### GetByIdAsync

1. **WithValidId_ReturnsProvider** - Verifies provider is returned for valid ID
2. **WithNonexistentId_ThrowsNotFoundException** - Verifies 404 for unknown ID
3. **WithDisabledProvider_ThrowsNotFoundException** - Verifies disabled providers are not accessible
