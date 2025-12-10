# ChatHub Unit Tests

Unit tests for the `ChatHub` SignalR hub class. These tests verify hub method behavior using mocked dependencies.

## Test Structure

```
Hubs/ChatHub/
├── ChatHubTestBase.cs          # Base class with mocks
├── JoinGroupTests.cs           # JoinGroup method tests
├── LeaveGroupTests.cs          # LeaveGroup method tests
├── StartTypingTests.cs         # StartTyping method tests
├── StopTypingTests.cs          # StopTyping method tests
├── OnConnectedAsyncTests.cs    # Connection lifecycle tests
├── OnDisconnectedAsyncTests.cs # Disconnection lifecycle tests
└── README.md                   # This file
```

## Running Tests

```bash
# Run all ChatHub tests
dotnet test tests/AiGroupChat.UnitTests --filter "FullyQualifiedName~Hubs.ChatHub"

# Run specific test file
dotnet test tests/AiGroupChat.UnitTests --filter "JoinGroupTests"
dotnet test tests/AiGroupChat.UnitTests --filter "OnConnectedAsyncTests"
```

## Test Coverage

### JoinGroupTests (4 tests)

| Test                                                | Description                             |
| --------------------------------------------------- | --------------------------------------- |
| `JoinGroup_WhenUserIsMember_AddsToSignalRGroup`     | Valid member joins successfully         |
| `JoinGroup_WhenUserIsNotMember_ThrowsHubException`  | Non-member gets rejected                |
| `JoinGroup_WhenUserIsNotMember_DoesNotAddToGroup`   | Verifies no group add on rejection      |
| `JoinGroup_VerifiesMembershipWithCorrectParameters` | Correct parameters passed to repository |

### LeaveGroupTests (2 tests)

| Test                                 | Description                    |
| ------------------------------------ | ------------------------------ |
| `LeaveGroup_RemovesFromSignalRGroup` | User successfully leaves group |
| `LeaveGroup_UsesCorrectGroupName`    | Correct group name format used |

### StartTypingTests (4 tests)

| Test                                               | Description                      |
| -------------------------------------------------- | -------------------------------- |
| `StartTyping_WhenMember_BroadcastsToOthersInGroup` | Broadcasts UserTyping event      |
| `StartTyping_WhenNotMember_DoesNotBroadcast`       | Silently ignores non-members     |
| `StartTyping_BroadcastsCorrectUserInfo`            | Event contains correct user data |
| `StartTyping_WhenUserNotFound_DoesNotBroadcast`    | Handles missing user gracefully  |

### StopTypingTests (3 tests)

| Test                                              | Description                           |
| ------------------------------------------------- | ------------------------------------- |
| `StopTyping_WhenMember_BroadcastsToOthersInGroup` | Broadcasts UserStoppedTyping event    |
| `StopTyping_WhenNotMember_DoesNotBroadcast`       | Silently ignores non-members          |
| `StopTyping_BroadcastsCorrectData`                | Event contains correct groupId/userId |

### OnConnectedAsyncTests (5 tests)

| Test                                                              | Description                           |
| ----------------------------------------------------------------- | ------------------------------------- |
| `OnConnectedAsync_AddsToPersonalChannel`                          | Auto-joins user-{userId} group        |
| `OnConnectedAsync_TracksConnection`                               | Calls ConnectionTracker.AddConnection |
| `OnConnectedAsync_FirstConnection_BroadcastsUserOnline`           | Presence broadcast on first connect   |
| `OnConnectedAsync_SecondConnection_DoesNotBroadcastOnline`        | No broadcast for additional tabs      |
| `OnConnectedAsync_FirstConnection_NoSharedUsers_DoesNotBroadcast` | No broadcast if no shared users       |

### OnDisconnectedAsyncTests (5 tests)

| Test                                                                | Description                              |
| ------------------------------------------------------------------- | ---------------------------------------- |
| `OnDisconnectedAsync_RemovesConnection`                             | Calls ConnectionTracker.RemoveConnection |
| `OnDisconnectedAsync_LastConnection_BroadcastsUserOffline`          | Presence broadcast on last disconnect    |
| `OnDisconnectedAsync_NotLastConnection_DoesNotBroadcast`            | No broadcast if other tabs open          |
| `OnDisconnectedAsync_LastConnection_NoSharedUsers_DoesNotBroadcast` | No broadcast if no shared users          |
| `OnDisconnectedAsync_WithException_StillRemovesConnection`          | Handles disconnect errors gracefully     |

## Mocking Strategy

The `ChatHubTestBase` provides:

- **Repository mocks**: `IGroupRepository`, `IUserRepository`, `IGroupMemberRepository`
- **Service mocks**: `IConnectionTracker`, `IChatHubService`, `ILogger<ChatHub>`
- **SignalR mocks**: `IHubCallerClients`, `IGroupManager`, `HubCallerContext`, `IClientProxy`

Hub context is injected via reflection since SignalR hub properties don't have public setters.
