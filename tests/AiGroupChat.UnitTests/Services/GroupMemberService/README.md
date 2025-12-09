# GroupMemberService Unit Tests

Unit tests for the `GroupMemberService` class which handles group member management operations.

## Structure

```
GroupMemberService/
├── GroupMemberServiceTestBase.cs          # Shared test setup and mocks
├── AddMemberAsyncTests.cs                 # Add member tests
├── GetMembersAsyncTests.cs                # Get members tests
├── UpdateMemberRoleAsyncTests.cs          # Update role tests
├── RemoveMemberAsyncTests.cs              # Remove member tests
├── LeaveGroupAsyncTests.cs                # Leave group tests
├── TransferOwnershipAsyncTests.cs         # Transfer ownership tests
├── MemberBroadcastTests.cs                # SignalR group channel broadcast tests
├── PersonalChannelNotificationTests.cs    # SignalR personal channel notification tests
└── README.md                              # This file
```

## Test Base Class

`GroupMemberServiceTestBase` provides shared setup for all test classes:

- `GroupRepositoryMock` - Mocked `IGroupRepository`
- `UserRepositoryMock` - Mocked `IUserRepository`
- `ChatHubServiceMock` - Mocked `IChatHubService`
- `GroupMemberService` - Instance under test with mocked dependencies

All test classes inherit from this base class.

## Test Coverage

| File                                  | Tests | Scenarios Covered                                                                                                 |
| ------------------------------------- | ----- | ----------------------------------------------------------------------------------------------------------------- |
| `AddMemberAsyncTests.cs`              | 5     | Valid add, nonexistent group, non-admin, nonexistent user, existing member                                        |
| `GetMembersAsyncTests.cs`             | 3     | Valid member, non-member, nonexistent group                                                                       |
| `UpdateMemberRoleAsyncTests.cs`       | 7     | Promote to admin, demote to member, non-owner, change owner role, invalid role, owner as role, nonexistent member |
| `RemoveMemberAsyncTests.cs`           | 6     | Owner removes member, owner removes admin, admin removes member, admin removes admin, remove owner, non-admin     |
| `LeaveGroupAsyncTests.cs`             | 5     | Member leaves, admin leaves, owner leaves, non-member, nonexistent group                                          |
| `TransferOwnershipAsyncTests.cs`      | 5     | Valid transfer, non-owner, non-member as new owner, transfer to self, nonexistent group                           |
| `MemberBroadcastTests.cs`             | 5     | MemberJoined, MemberLeft (remove), MemberLeft (leave), MemberRoleChanged, Transfer broadcasts both                |
| `PersonalChannelNotificationTests.cs` | 4     | AddedToGroup, RemovedFromGroup, RoleChanged, LeaveGroup no notification                                           |

**Total: 40 tests**

## Running Tests

```bash
# Run all tests
dotnet test

# Run only GroupMemberService tests
dotnet test --filter "FullyQualifiedName~GroupMemberService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~AddMemberAsyncTests"
dotnet test --filter "FullyQualifiedName~MemberBroadcastTests"
dotnet test --filter "FullyQualifiedName~PersonalChannelNotificationTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~AddMemberAsyncTests.WithValidRequest_AddsMemberAndReturnsResponse"
```

## Test Patterns

### Naming Convention

Tests follow the pattern: `MethodCondition_ExpectedResult`

Examples:

- `WithValidRequest_AddsMemberAndReturnsResponse`
- `WithNonOwner_ThrowsAuthorizationException`
- `WithOwnerPromotingMemberToAdmin_UpdatesRole`

## SignalR Event Tests

### Group Channel Events (MemberBroadcastTests)

| Event               | Test                                                       |
| ------------------- | ---------------------------------------------------------- |
| `MemberJoined`      | `AddMemberAsync_BroadcastsMemberJoined`                    |
| `MemberLeft`        | `RemoveMemberAsync_BroadcastsMemberLeft`                   |
| `MemberLeft`        | `LeaveGroupAsync_BroadcastsMemberLeft`                     |
| `MemberRoleChanged` | `UpdateMemberRoleAsync_BroadcastsMemberRoleChanged`        |
| `MemberRoleChanged` | `TransferOwnershipAsync_BroadcastsRoleChangesForBothUsers` |

### Personal Channel Events (PersonalChannelNotificationTests)

| Event              | Test                                                  |
| ------------------ | ----------------------------------------------------- |
| `AddedToGroup`     | `AddMemberAsync_SendsAddedToGroupNotification`        |
| `RemovedFromGroup` | `RemoveMemberAsync_SendsRemovedFromGroupNotification` |
| `RoleChanged`      | `UpdateMemberRoleAsync_SendsRoleChangedNotification`  |
| (none)             | `LeaveGroupAsync_DoesNotSendPersonalNotification`     |

## Authorization Tests

| Scenario                         | Expected Behavior               |
| -------------------------------- | ------------------------------- |
| Non-admin adding member          | Throws `AuthorizationException` |
| Non-member getting members       | Throws `AuthorizationException` |
| Non-owner updating role          | Throws `AuthorizationException` |
| Admin removing another admin     | Throws `AuthorizationException` |
| Non-admin removing member        | Throws `AuthorizationException` |
| Non-owner transferring ownership | Throws `AuthorizationException` |

## Role Hierarchy Tests

| Rule                              | Test                                                                         |
| --------------------------------- | ---------------------------------------------------------------------------- |
| Only owner can promote/demote     | `UpdateMemberRoleAsyncTests.WithNonOwner_ThrowsAuthorizationException`       |
| Only owner can remove admins      | `RemoveMemberAsyncTests.WithAdminRemovingAdmin_ThrowsAuthorizationException` |
| Owner cannot leave                | `LeaveGroupAsyncTests.WithOwner_ThrowsValidationException`                   |
| Owner cannot be removed           | `RemoveMemberAsyncTests.WithRemovingOwner_ThrowsValidationException`         |
| Owner role cannot be changed      | `UpdateMemberRoleAsyncTests.WithChangingOwnerRole_ThrowsValidationException` |
| Only owner can transfer ownership | `TransferOwnershipAsyncTests.WithNonOwner_ThrowsAuthorizationException`      |
