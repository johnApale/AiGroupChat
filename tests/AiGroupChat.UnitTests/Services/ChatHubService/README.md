# ChatHubService Unit Tests

Unit tests for the `ChatHubService` class which handles broadcasting SignalR events via `IHubContext<ChatHub>`.

## Test Structure

```
Services/ChatHubService/
├── ChatHubServiceTestBase.cs       # Base class with IHubContext mock
├── BroadcastMessageTests.cs        # Message broadcasting
├── BroadcastAiSettingsTests.cs     # AI settings broadcasting
├── BroadcastMemberEventsTests.cs   # Member join/leave/role
├── BroadcastTypingTests.cs         # Typing indicators
├── PersonalChannelTests.cs         # Personal notifications
├── PresenceTests.cs                # Online/offline broadcasts
└── README.md                       # This file
```

## Running Tests

```bash
# Run all ChatHubService tests
dotnet test tests/AiGroupChat.UnitTests --filter "FullyQualifiedName~Services.ChatHubService"

# Run specific test file
dotnet test tests/AiGroupChat.UnitTests --filter "BroadcastMessageTests"
dotnet test tests/AiGroupChat.UnitTests --filter "PresenceTests"
```

## Test Coverage

### BroadcastMessageTests (3 tests)

| Test                                          | Description                         |
| --------------------------------------------- | ----------------------------------- |
| `BroadcastMessageAsync_SendsToCorrectGroup`   | Message goes to group-{groupId}     |
| `BroadcastMessageAsync_SendsCorrectEventName` | Event name is "MessageReceived"     |
| `BroadcastMessageAsync_SendsCorrectPayload`   | MessageResponse is passed correctly |

### BroadcastAiSettingsTests (3 tests)

| Test                                                    | Description                    |
| ------------------------------------------------------- | ------------------------------ |
| `BroadcastAiSettingsChangedAsync_SendsToCorrectGroup`   | Settings go to group-{groupId} |
| `BroadcastAiSettingsChangedAsync_SendsCorrectEventName` | Event is "AiSettingsChanged"   |
| `BroadcastAiSettingsChangedAsync_SendsCorrectPayload`   | AiSettingsChangedEvent passed  |

### BroadcastMemberEventsTests (6 tests)

| Test                                                    | Description                        |
| ------------------------------------------------------- | ---------------------------------- |
| `BroadcastMemberJoinedAsync_SendsToCorrectGroup`        | MemberJoined to correct group      |
| `BroadcastMemberJoinedAsync_SendsCorrectEventName`      | Event name verification            |
| `BroadcastMemberLeftAsync_SendsToCorrectGroup`          | MemberLeft to correct group        |
| `BroadcastMemberLeftAsync_SendsCorrectEventName`        | Event name verification            |
| `BroadcastMemberRoleChangedAsync_SendsToCorrectGroup`   | MemberRoleChanged to correct group |
| `BroadcastMemberRoleChangedAsync_SendsCorrectEventName` | Event name verification            |

### BroadcastTypingTests (6 tests)

| Test                                                    | Description                |
| ------------------------------------------------------- | -------------------------- |
| `BroadcastUserTypingAsync_SendsToCorrectGroup`          | UserTyping to group        |
| `BroadcastUserTypingAsync_SendsCorrectEventName`        | Event name verification    |
| `BroadcastUserTypingAsync_SendsCorrectPayload`          | Typing event data          |
| `BroadcastUserStoppedTypingAsync_SendsToCorrectGroup`   | UserStoppedTyping to group |
| `BroadcastUserStoppedTypingAsync_SendsCorrectEventName` | Event name verification    |
| `BroadcastUserStoppedTypingAsync_SendsCorrectPayload`   | Stopped typing event data  |

### PersonalChannelTests (10 tests)

| Test                                                     | Description               |
| -------------------------------------------------------- | ------------------------- |
| `SendGroupActivityAsync_SendsToPersonalChannel`          | Activity to user-{userId} |
| `SendGroupActivityAsync_SendsCorrectEventName`           | "GroupActivity" event     |
| `SendNewMessageNotificationAsync_SendsToPersonalChannel` | Notification to user      |
| `SendNewMessageNotificationAsync_SendsCorrectEventName`  | "NewMessageNotification"  |
| `SendAddedToGroupAsync_SendsToPersonalChannel`           | Added notification        |
| `SendAddedToGroupAsync_SendsCorrectEventName`            | "AddedToGroup" event      |
| `SendRemovedFromGroupAsync_SendsToPersonalChannel`       | Removed notification      |
| `SendRemovedFromGroupAsync_SendsCorrectEventName`        | "RemovedFromGroup" event  |
| `SendRoleChangedAsync_SendsToPersonalChannel`            | Role change notification  |
| `SendRoleChangedAsync_SendsCorrectEventName`             | "RoleChanged" event       |

### PresenceTests (6 tests)

| Test                                         | Description                 |
| -------------------------------------------- | --------------------------- |
| `SendUserOnlineAsync_SendsToMultipleUsers`   | Online to all shared users  |
| `SendUserOnlineAsync_SendsCorrectEventName`  | "UserOnline" event          |
| `SendUserOnlineAsync_EmptyList_DoesNotSend`  | Handles no shared users     |
| `SendUserOfflineAsync_SendsToMultipleUsers`  | Offline to all shared users |
| `SendUserOfflineAsync_SendsCorrectEventName` | "UserOffline" event         |
| `SendUserOfflineAsync_EmptyList_DoesNotSend` | Handles no shared users     |

## Mocking Strategy

The `ChatHubServiceTestBase` provides:

- **IHubContext<ChatHub>** mock with captured method calls
- **IHubClients** mock for accessing group/groups clients
- **IClientProxy** mocks for single group and multiple groups
- Capture fields for verifying sent method names, arguments, and group names

All broadcasts use `SendCoreAsync` internally, which is captured for verification.
