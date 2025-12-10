# ChatHub Integration Tests

End-to-end integration tests for SignalR functionality. These tests use real WebSocket connections against the test server with a PostgreSQL database via Testcontainers.

## Test Structure

```
Hubs/ChatHub/
├── ConnectionTests.cs          # WebSocket connection/auth tests
├── JoinLeaveGroupTests.cs      # Joining/leaving SignalR groups
├── TypingIndicatorTests.cs     # Typing indicator broadcasts
├── MessageBroadcastTests.cs    # Message delivery via WebSocket
├── MemberEventTests.cs         # Member join/leave/role events
├── AiSettingsEventTests.cs     # AI settings change broadcasts
├── PresenceTests.cs            # Online/offline presence
└── README.md                   # This file
```

## Running Tests

```bash
# Run all SignalR integration tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Hubs.ChatHub"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "ConnectionTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "MessageBroadcastTests"

# Run with detailed output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Hubs.ChatHub" --logger "console;verbosity=detailed"
```

**Note:** Docker must be running (tests use Testcontainers for PostgreSQL).

## Test Coverage

### ConnectionTests (3 tests)

| Test                              | Description                    |
| --------------------------------- | ------------------------------ |
| `Connect_WithValidToken_Succeeds` | Authenticated connection works |
| `Connect_WithInvalidToken_Fails`  | Invalid JWT rejected           |
| `Connect_WithoutToken_Fails`      | Missing token rejected         |

### JoinLeaveGroupTests (6 tests)

| Test                                         | Description                       |
| -------------------------------------------- | --------------------------------- |
| `JoinGroup_WhenMember_Succeeds`              | Member can join SignalR group     |
| `JoinGroup_WhenNotMember_ThrowsHubException` | Non-member rejected               |
| `JoinGroup_ReceivesSubsequentMessages`       | Messages received after joining   |
| `LeaveGroup_StopsReceivingMessages`          | No messages after leaving         |
| `JoinGroup_MultipleGroups_ReceivesFromAll`   | Multiple group subscriptions work |
| `JoinGroup_OnlyJoinedUserReceivesMessages`   | Isolation between users           |

### TypingIndicatorTests (5 tests)

| Test                                       | Description                |
| ------------------------------------------ | -------------------------- |
| `StartTyping_OtherMembersReceiveEvent`     | Typing broadcast to others |
| `StartTyping_SenderDoesNotReceiveOwnEvent` | No self-echo               |
| `StopTyping_OtherMembersReceiveEvent`      | Stop typing broadcast      |
| `StartTyping_NonMember_NoEventBroadcast`   | Non-members ignored        |
| `TypingEvent_ContainsCorrectUserInfo`      | Event data validation      |

### MessageBroadcastTests (7 tests)

| Test                                           | Description             |
| ---------------------------------------------- | ----------------------- |
| `SendMessage_JoinedMembersReceiveViaWebSocket` | Basic message delivery  |
| `SendMessage_MessageContainsAllFields`         | Full message data       |
| `SendMessage_MultipleJoinedUsers_AllReceive`   | Broadcast to all        |
| `SendMessage_SenderAlsoReceives`               | Sender gets own message |
| `SendMessage_NonGroupMembers_DoNotReceive`     | Group isolation         |
| `SendMessage_NotJoinedSignalR_DoesNotReceive`  | Must join SignalR group |
| `SendMessage_RapidMessages_AllReceived`        | Multiple messages work  |

### MemberEventTests (8 tests)

| Test                                                 | Description                      |
| ---------------------------------------------------- | -------------------------------- |
| `AddMember_JoinedMembersReceiveMemberJoined`         | MemberJoined on group channel    |
| `AddMember_NewMemberReceivesAddedToGroup`            | AddedToGroup on personal channel |
| `RemoveMember_JoinedMembersReceiveMemberLeft`        | MemberLeft on group channel      |
| `RemoveMember_RemovedMemberReceivesRemovedFromGroup` | RemovedFromGroup on personal     |
| `UpdateRole_JoinedMembersReceiveMemberRoleChanged`   | Role change on group channel     |
| `UpdateRole_AffectedMemberReceivesRoleChanged`       | RoleChanged on personal channel  |
| `LeaveGroup_MembersReceiveMemberLeft`                | Self-leave broadcasts            |
| `TransferOwnership_BroadcastsRoleChanges`            | Ownership transfer notifications |

### AiSettingsEventTests (4 tests)

| Test                                                  | Description         |
| ----------------------------------------------------- | ------------------- |
| `UpdateAiSettings_JoinedMembersReceiveEvent`          | AI toggle broadcast |
| `UpdateAiSettings_EventContainsAllFields`             | Full settings data  |
| `UpdateAiSettings_DisableMonitoring_BroadcastsChange` | Disable broadcasts  |
| `UpdateAiSettings_NonJoinedMembers_DoNotReceive`      | Must join group     |

### PresenceTests (7 tests)

| Test                                                  | Description               |
| ----------------------------------------------------- | ------------------------- |
| `Connect_FirstConnection_SharedUsersReceiveOnline`    | Online broadcast          |
| `Disconnect_LastConnection_SharedUsersReceiveOffline` | Offline broadcast         |
| `Connect_NoSharedGroups_NoOnlineBroadcast`            | No broadcast to strangers |
| `OnlineEvent_ContainsCorrectUserInfo`                 | Online event data         |
| `OfflineEvent_ContainsCorrectUserInfo`                | Offline event data        |
| `MultipleSharedGroups_OnlyOneOnlineEvent`             | No duplicate broadcasts   |
| `Presence_OnlyToUsersInSharedGroups`                  | Presence isolation        |

## Test Infrastructure

### SignalRHelper

Manages a single SignalR connection with:

- Event collectors for all 14 event types
- `WaitFor*Async()` methods with 5-second timeout
- Hub method wrappers (`JoinGroupAsync`, `StartTypingAsync`, etc.)

### SignalRIntegrationTestBase

Base class providing:

- `CreateSignalRConnectionAsync(token)` - Creates connected helper
- Automatic connection cleanup on dispose
- Inherits from `IntegrationTestBase` for HTTP helpers

### Sequential Execution

All SignalR tests use `[Collection("SignalR")]` for sequential execution to avoid:

- ConnectionTracker singleton conflicts
- Database race conditions
- Flaky parallel WebSocket tests

## Timeout Configuration

Default timeout for waiting on events: **5 seconds**

This balances:

- Fast failure for broken functionality
- Tolerance for CI environment variability
