# UserService Unit Tests

Unit tests for the `UserService` class which handles user profile retrieval.

## Structure

```
UserService/
├── UserServiceTestBase.cs      # Shared test setup and mocks
├── GetByIdAsyncTests.cs        # Get user by ID tests
├── GetCurrentUserAsyncTests.cs # Get current user tests
└── README.md                   # This file
```

## Test Base Class

`UserServiceTestBase` provides shared setup for all test classes:

- `UserRepositoryMock` - Mocked `IUserRepository`
- `UserService` - Instance under test with mocked dependencies

All test classes inherit from this base class.

## Test Coverage

| File                          | Tests | Scenarios Covered                |
| ----------------------------- | ----- | -------------------------------- |
| `GetByIdAsyncTests.cs`        | 2     | Valid ID, nonexistent ID         |
| `GetCurrentUserAsyncTests.cs` | 2     | Valid current user, invalid user |

**Total: 4 tests**

## Running Tests

```bash
# Run all tests
dotnet test

# Run only UserService tests
dotnet test --filter "FullyQualifiedName~UserService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~GetByIdAsyncTests"
```

## Test Patterns

### Naming Convention

Tests follow the pattern: `MethodCondition_ExpectedResult`

Examples:

- `WithValidId_ReturnsUserResponse`
- `WithNonexistentId_ThrowsNotFoundException`

### Arrange-Act-Assert

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task WithValidId_ReturnsUserResponse()
{
    // Arrange - Set up mocks and test data
    User user = new User { ... };
    UserRepositoryMock.Setup(...);

    // Act - Call the method under test
    UserResponse result = await UserService.GetByIdAsync(user.Id);

    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
}
```
