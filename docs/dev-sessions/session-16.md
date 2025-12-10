# Session 16: SignalR Testing Implementation

## Overview

This session implemented comprehensive testing for the SignalR real-time messaging functionality, including unit tests for `ChatHub` and `ChatHubService`, and full integration tests for WebSocket communication.

## What Was Accomplished

### 1. SignalR Test Infrastructure

Created the foundational infrastructure for SignalR testing:

**Integration Test Infrastructure:**

- `SignalRCollection.cs` - xUnit collection for sequential test execution
- `SignalRIntegrationTestBase.cs` - Base class with connection factory and auto-cleanup
- `SignalRHelper.cs` - Helper class managing WebSocket connections, event collection, and waiting utilities

**Key Design Decisions:**

- **5-second timeout** for WebSocket event waiting (balances CI needs with fast failure)
- **Sequential execution** via `[Collection("SignalR")]` to avoid ConnectionTracker conflicts
- **Factory pattern** for creating multiple connections with automatic disposal tracking
- **HttpMessageHandler injection** for in-memory test server communication

### 2. ChatHub Unit Tests (23 tests)

Created unit tests for all hub methods with mocked dependencies:

| Test File                     | Tests | Coverage                                  |
| ----------------------------- | ----- | ----------------------------------------- |
| `JoinGroupTests.cs`           | 4     | Member validation, SignalR group add      |
| `LeaveGroupTests.cs`          | 2     | SignalR group removal                     |
| `StartTypingTests.cs`         | 4     | Typing broadcast, user validation         |
| `StopTypingTests.cs`          | 3     | Stop typing broadcast                     |
| `OnConnectedAsyncTests.cs`    | 5     | Connection tracking, presence broadcast   |
| `OnDisconnectedAsyncTests.cs` | 5     | Disconnection handling, offline broadcast |

**Base Class:** `ChatHubTestBase.cs` provides:

- Mocked repositories (`IGroupRepository`, `IUserRepository`, `IGroupMemberRepository`)
- Mocked services (`IConnectionTracker`, `IChatHubService`, `ILogger`)
- Mocked SignalR infrastructure (`IHubCallerClients`, `IGroupManager`, `HubCallerContext`)
- Hub context injection via reflection

### 3. ChatHubService Unit Tests (34 tests)

Created unit tests for all service broadcast methods:

| Test File                       | Tests | Coverage                      |
| ------------------------------- | ----- | ----------------------------- |
| `BroadcastMessageTests.cs`      | 3     | Message delivery to groups    |
| `BroadcastAiSettingsTests.cs`   | 3     | AI settings change broadcasts |
| `BroadcastMemberEventsTests.cs` | 6     | Member join/leave/role events |
| `BroadcastTypingTests.cs`       | 6     | Typing indicator broadcasts   |
| `PersonalChannelTests.cs`       | 10    | Personal notifications        |
| `PresenceTests.cs`              | 6     | Online/offline broadcasts     |

**Base Class:** `ChatHubServiceTestBase.cs` provides:

- Mocked `IHubContext<ChatHub>` with captured method calls
- Mocked `IHubClients` for group/groups client access
- Capture fields for verifying sent methods, arguments, and group names

### 4. SignalR Integration Tests (40 tests)

Created end-to-end tests with real WebSocket connections:

| Test File                  | Tests | Coverage                            |
| -------------------------- | ----- | ----------------------------------- |
| `ConnectionTests.cs`       | 3     | Auth, valid/invalid tokens          |
| `JoinLeaveGroupTests.cs`   | 6     | Group subscription, message routing |
| `TypingIndicatorTests.cs`  | 5     | Typing broadcasts between users     |
| `MessageBroadcastTests.cs` | 7     | Message delivery via WebSocket      |
| `MemberEventTests.cs`      | 8     | Member join/leave/role events       |
| `AiSettingsEventTests.cs`  | 4     | AI settings change broadcasts       |
| `PresenceTests.cs`         | 7     | Online/offline presence             |

### 5. Bug Fix: TransferOwnership Missing Notifications

Discovered and fixed a bug where `TransferOwnershipAsync` wasn't sending personal channel notifications:

**Before:** Only sent `MemberRoleChangedEvent` to group channel
**After:** Also sends `RoleChangedEvent` to personal channels for both old owner (now Admin) and new owner

```csharp
// Added to GroupMemberService.TransferOwnershipAsync
RoleChangedEvent newOwnerPersonalEvent = new RoleChangedEvent
{
    GroupId = groupId,
    GroupName = group.Name,
    OldRole = newOwnerOldRole,
    NewRole = GroupRole.Owner.ToString(),
    ChangedByName = currentMember.User?.DisplayName ?? ...,
    ChangedAt = now
};
await _chatHubService.SendRoleChangedAsync(request.NewOwnerUserId, newOwnerPersonalEvent, cancellationToken);
```

### 6. Test Helper Enhancements

Added missing convenience methods to existing helpers:

**GroupMemberHelper:**

- `RemoveMemberAsync(groupId, memberId)` - Remove member (throws on failure)
- `LeaveGroupAsync(groupId)` - Leave group (throws on failure)

**GroupHelper:**

- `UpdateAiSettingsAsync(groupId, aiMonitoringEnabled, aiProviderId)` - Update AI settings with response

## Files Created

### Unit Tests

```
tests/AiGroupChat.UnitTests/
├── Hubs/ChatHub/
│   ├── ChatHubTestBase.cs
│   ├── JoinGroupTests.cs
│   ├── LeaveGroupTests.cs
│   ├── StartTypingTests.cs
│   ├── StopTypingTests.cs
│   ├── OnConnectedAsyncTests.cs
│   ├── OnDisconnectedAsyncTests.cs
│   └── README.md
└── Services/ChatHubService/
    ├── ChatHubServiceTestBase.cs
    ├── BroadcastMessageTests.cs
    ├── BroadcastAiSettingsTests.cs
    ├── BroadcastMemberEventsTests.cs
    ├── BroadcastTypingTests.cs
    ├── PersonalChannelTests.cs
    ├── PresenceTests.cs
    └── README.md
```

### Integration Tests

```
tests/AiGroupChat.IntegrationTests/
├── Infrastructure/
│   ├── SignalRCollection.cs
│   └── SignalRIntegrationTestBase.cs
├── Helpers/
│   └── SignalRHelper.cs
└── Hubs/ChatHub/
    ├── ConnectionTests.cs
    ├── JoinLeaveGroupTests.cs
    ├── TypingIndicatorTests.cs
    ├── MessageBroadcastTests.cs
    ├── MemberEventTests.cs
    ├── AiSettingsEventTests.cs
    ├── PresenceTests.cs
    └── README.md
```

## Files Modified

- `src/AiGroupChat.Application/Services/GroupMemberService.cs` - Added personal channel notifications to TransferOwnershipAsync
- `tests/AiGroupChat.IntegrationTests/AiGroupChat.IntegrationTests.csproj` - Added SignalR.Client package
- `tests/AiGroupChat.IntegrationTests/Helpers/GroupHelper.cs` - Added UpdateAiSettingsAsync
- `tests/AiGroupChat.IntegrationTests/Helpers/GroupMemberHelper.cs` - Added RemoveMemberAsync, LeaveGroupAsync
- `tests/AiGroupChat.IntegrationTests/README.md` - Updated with SignalR test documentation

## Test Summary

| Category                  | Tests  |
| ------------------------- | ------ |
| ChatHub Unit Tests        | 23     |
| ChatHubService Unit Tests | 34     |
| SignalR Integration Tests | 40     |
| **Total New Tests**       | **97** |

### Full Project Test Count

| Category                | Tests  |
| ----------------------- | ------ |
| REST API Integration    | 106    |
| SignalR Integration     | 40     |
| Unit Tests (Auth, etc.) | 57+    |
| **SignalR Unit Tests**  | **57** |

## Running the Tests

```bash
# Run all SignalR-related tests
dotnet test --filter "FullyQualifiedName~ChatHub or FullyQualifiedName~ChatHubService"

# Run only unit tests
dotnet test tests/AiGroupChat.UnitTests --filter "FullyQualifiedName~Hubs.ChatHub or FullyQualifiedName~Services.ChatHubService"

# Run only integration tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Hubs.ChatHub"

# Run specific test file
dotnet test --filter "ConnectionTests"
dotnet test --filter "MessageBroadcastTests"
dotnet test --filter "PresenceTests"
```

## Key Technical Details

### SignalRHelper Event Collection

The helper collects 14 different event types:

- `MessageReceived` → `ReceivedMessages`
- `UserTyping` → `TypingEvents`
- `UserStoppedTyping` → `StoppedTypingEvents`
- `MemberJoined` → `MemberJoinedEvents`
- `MemberLeft` → `MemberLeftEvents`
- `MemberRoleChanged` → `MemberRoleChangedEvents`
- `AiSettingsChanged` → `AiSettingsChangedEvents`
- `UserOnline` → `UserOnlineEvents`
- `UserOffline` → `UserOfflineEvents`
- `AddedToGroup` → `AddedToGroupEvents`
- `RemovedFromGroup` → `RemovedFromGroupEvents`
- `RoleChanged` → `RoleChangedEvents`
- `GroupActivity` → `GroupActivityEvents`
- `NewMessageNotification` → `NewMessageNotificationEvents`

### Wait Pattern

```csharp
private async Task<T> WaitForEventAsync<T>(List<T> eventList, Func<T, bool> predicate, TimeSpan? timeout)
{
    DateTime deadline = DateTime.UtcNow + (timeout ?? _defaultTimeout);
    while (DateTime.UtcNow < deadline)
    {
        T? existing = eventList.FirstOrDefault(predicate);
        if (existing != null) return existing;
        await Task.Delay(50); // Poll every 50ms
    }
    throw new TimeoutException(...);
}
```

### Test Server WebSocket Integration

```csharp
// Key: Use test server's HttpMessageHandler for in-memory routing
HttpMessageHandler handler = Factory.Server.CreateHandler();
_connection = new HubConnectionBuilder()
    .WithUrl(urlWithToken, options =>
    {
        options.HttpMessageHandlerFactory = _ => handler;
    })
    .Build();
```

## Next Steps

Potential future enhancements:

1. Add stress tests for high message volume
2. Test reconnection scenarios
3. Add tests for AI typing indicators (when AI service is integrated)
4. Test SignalR backplane scenarios (Redis) for horizontal scaling
