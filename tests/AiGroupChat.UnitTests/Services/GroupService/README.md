# GroupService Unit Tests

Unit tests for the `GroupService` class which handles group CRUD operations and authorization.

## Structure

```
GroupService/
├── GroupServiceTestBase.cs     # Shared test setup and mocks
├── CreateAsyncTests.cs         # Create group tests
├── GetMyGroupsAsyncTests.cs    # List user's groups tests
├── GetByIdAsyncTests.cs        # Get group by ID tests
├── UpdateAsyncTests.cs         # Update group tests
├── DeleteAsyncTests.cs         # Delete group tests
└── README.md                   # This file
```

## Test Base Class

`GroupServiceTestBase` provides shared setup for all test classes:

- `GroupRepositoryMock` - Mocked `IGroupRepository`
- `AiProviderRepositoryMock` - Mocked `IAiProviderRepository`
- `DefaultAiProvider` - Test AI provider for group creation
- `GroupService` - Instance under test with mocked dependencies

All test classes inherit from this base class.

## Test Coverage

| File                       | Tests | Scenarios Covered                                                                   |
| -------------------------- | ----- | ----------------------------------------------------------------------------------- |
| `CreateAsyncTests.cs`      | 4     | Valid creation, creator becomes owner, assigns default provider, no providers error |
| `GetMyGroupsAsyncTests.cs` | 2     | Returns groups, returns empty list                                                  |
| `GetByIdAsyncTests.cs`     | 3     | Valid member access, nonexistent group, non-member                                  |
| `UpdateAsyncTests.cs`      | 3     | Valid admin update, nonexistent group, non-admin                                    |
| `DeleteAsyncTests.cs`      | 3     | Valid owner delete, nonexistent group, non-owner                                    |

**Total: 15 tests**

## Running Tests

```bash
# Run all tests
dotnet test

# Run only GroupService tests
dotnet test --filter "FullyQualifiedName~GroupService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~CreateAsyncTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~CreateAsyncTests.WithValidRequest_CreatesGroupAndReturnsResponse"
```

## Test Patterns

### Naming Convention

Tests follow the pattern: `MethodCondition_ExpectedResult`

Examples:

- `WithValidRequest_CreatesGroupAndReturnsResponse`
- `WithNonexistentGroup_ThrowsNotFoundException`
- `WithNonOwner_ThrowsAuthorizationException`

### Arrange-Act-Assert

All tests follow the AAA pattern:

```csharp
[Fact]
public async Task WithValidRequestAndAdmin_UpdatesAndReturnsGroup()
{
    // Arrange - Set up mocks and test data
    var request = new UpdateGroupRequest { ... };
    GroupRepositoryMock.Setup(...);

    // Act - Call the method under test
    var result = await GroupService.UpdateAsync(groupId, request, currentUserId);

    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal(request.Name, result.Name);
}
```

## Authorization Tests

Several tests verify authorization behavior:

| Scenario                 | Expected Behavior               |
| ------------------------ | ------------------------------- |
| Non-member viewing group | Throws `AuthorizationException` |
| Non-admin updating group | Throws `AuthorizationException` |
| Non-owner deleting group | Throws `AuthorizationException` |
| Admin updating group     | Success                         |
| Owner deleting group     | Success                         |

## Role Hierarchy

| Role   | Permissions                                          |
| ------ | ---------------------------------------------------- |
| Owner  | All permissions, transfer ownership, delete group    |
| Admin  | Add/remove members, update group, change AI settings |
| Member | View group, send messages, leave group               |

## Business Rule Tests

| Rule                        | Test                                                           |
| --------------------------- | -------------------------------------------------------------- |
| Creator becomes owner       | `CreateAsyncTests.WithValidRequest_AddsCreatorAsOwner`         |
| Only members can view group | `GetByIdAsyncTests.WithNonMember_ThrowsAuthorizationException` |
| Only admins can update      | `UpdateAsyncTests.WithNonAdmin_ThrowsAuthorizationException`   |
| Only owner can delete       | `DeleteAsyncTests.WithNonOwner_ThrowsAuthorizationException`   |
