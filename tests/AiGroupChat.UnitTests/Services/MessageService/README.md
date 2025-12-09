# MessageService Unit Tests

Unit tests for the `MessageService` class which handles message sending and retrieval.

## Structure

```
MessageService/
├── MessageServiceTestBase.cs              # Shared test setup and mocks
├── SendMessageAsyncTests.cs               # Send message tests
├── SendMessageBroadcastTests.cs           # SignalR group channel broadcast tests
├── SendMessagePersonalNotificationTests.cs # SignalR personal channel notification tests
├── GetMessagesAsyncTests.cs               # Get paginated messages tests
└── README.md                              # This file
```

## Test Base Class

`MessageServiceTestBase` provides shared setup for all test classes:

- `MessageRepositoryMock` - Mocked `IMessageRepository`
- `GroupRepositoryMock` - Mocked `IGroupRepository`
- `GroupMemberRepositoryMock` - Mocked `IGroupMemberRepository`
- `ChatHubServiceMock` - Mocked `IChatHubService`
- `TestUser` - Test user for message sending
- `TestGroup` - Test group with AI provider
- `MessageService` - Instance under test with mocked dependencies
- `CreateTestMessage()` - Helper to create test message instances

All test classes inherit from this base class.

## Test Coverage

| File                                      | Tests | Scenarios Covered                                                                               |
| ----------------------------------------- | ----- | ----------------------------------------------------------------------------------------------- |
| `SendMessageAsyncTests.cs`                | 4     | Valid send, AI visible when monitoring on, nonexistent group, non-member                        |
| `SendMessageBroadcastTests.cs`            | 2     | Broadcasts message to group, correct message response                                           |
| `SendMessagePersonalNotificationTests.cs` | 4     | GroupActivity to others, NewMessageNotification, preview truncation, no notifications when solo |
| `GetMessagesAsyncTests.cs`                | 6     | Valid retrieval, pagination, empty group, nonexistent group, non-member, page size clamping     |

**Total: 16 tests**

## Running Tests

```bash
# Run all tests
dotnet test

# Run only MessageService tests
dotnet test --filter "FullyQualifiedName~MessageService"

# Run specific test file
dotnet test --filter "FullyQualifiedName~SendMessageAsyncTests"
dotnet test --filter "FullyQualifiedName~SendMessageBroadcastTests"
dotnet test --filter "FullyQualifiedName~SendMessagePersonalNotificationTests"
dotnet test --filter "FullyQualifiedName~GetMessagesAsyncTests"
```

## Key Behaviors Tested

### SendMessageAsync

- Creates message with correct sender info
- Sets `AiVisible = true` when group's `AiMonitoringEnabled = true`
- Sets `AiVisible = false` when group's `AiMonitoringEnabled = false`
- Returns 404 for nonexistent group
- Returns 403 for non-member

### SignalR Group Channel Broadcasts

- Broadcasts `MessageReceived` event to group channel
- Message response contains correct content, groupId, senderId

### SignalR Personal Channel Notifications

- Sends `GroupActivity` event to all members except sender
- Sends `NewMessageNotification` event to all members except sender
- Events include GroupName, ActivityType, SenderName, Preview
- Long messages are truncated to 50 characters with "..."
- No notifications sent when sender is only member

### GetMessagesAsync

- Returns paginated messages (newest first)
- Correct pagination metadata (page, totalCount, hasNextPage, etc.)
- Returns empty list for group with no messages
- Clamps pageSize to maximum of 100
- Returns 404 for nonexistent group
- Returns 403 for non-member
