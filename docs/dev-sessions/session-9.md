# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 9: SignalR Real-time Messaging

**Date:** December 8, 2025

### Completed

#### 1. SignalR Hub Infrastructure

Added WebSocket support for real-time messaging using SignalR.

| Endpoint     | Description                             |
| ------------ | --------------------------------------- |
| `/hubs/chat` | SignalR hub for real-time communication |

**Client → Server Methods:**

| Method                 | Description                           |
| ---------------------- | ------------------------------------- |
| `JoinGroup(groupId)`   | Subscribe to group's real-time events |
| `LeaveGroup(groupId)`  | Unsubscribe from group events         |
| `StartTyping(groupId)` | Notify group that user started typing |
| `StopTyping(groupId)`  | Notify group that user stopped typing |

**Server → Client Events:**

| Event               | Description                    |
| ------------------- | ------------------------------ |
| `MessageReceived`   | New message sent in group      |
| `AiSettingsChanged` | Group AI settings were updated |
| `MemberAdded`       | New member joined the group    |
| `MemberRemoved`     | Member left or was removed     |
| `MemberRoleChanged` | Member's role was changed      |
| `UserTyping`        | User started typing in group   |
| `UserStoppedTyping` | User stopped typing            |

#### 2. Files Created

**Application Layer (DTOs & Interface):**

- `src/AiGroupChat.Application/DTOs/SignalR/AiSettingsChangedEvent.cs`
- `src/AiGroupChat.Application/DTOs/SignalR/UserTypingEvent.cs`
- `src/AiGroupChat.Application/DTOs/SignalR/MemberEvent.cs` (MemberAddedEvent, MemberRemovedEvent, MemberRoleChangedEvent)
- `src/AiGroupChat.Application/Interfaces/IChatHubService.cs`

**API Layer (Hub & Service):**

- `src/AiGroupChat.API/Hubs/ChatHub.cs`
- `src/AiGroupChat.API/Services/ChatHubService.cs`

#### 3. Files Modified

- `src/AiGroupChat.API/Program.cs` - Added SignalR services, JWT WebSocket auth, hub endpoint mapping
- `src/AiGroupChat.Application/Services/MessageService.cs` - Inject `IChatHubService`, broadcast on send
- `src/AiGroupChat.Application/Services/GroupService.cs` - Broadcast AI settings changes
- `src/AiGroupChat.Application/Services/GroupMemberService.cs` - Broadcast member add/remove/role changes

#### 4. Unit Tests

| File                                | Tests | Scenarios Covered                                                       |
| ----------------------------------- | ----- | ----------------------------------------------------------------------- |
| `SendMessageBroadcastTests.cs`      | 2     | Message broadcast on send, correct message data                         |
| `UpdateAiSettingsBroadcastTests.cs` | 2     | AI settings broadcast, correct provider info                            |
| `MemberBroadcastTests.cs`           | 5     | Add member, remove member, leave group, role change, transfer ownership |

**Test totals:** 107 total (98 previous + 9 new)

#### 5. Documentation Updates

- `src/AiGroupChat.API/README.md` - Added SignalR hub documentation
- `src/AiGroupChat.Application/README.md` - Added `IChatHubService` interface

### Architecture Decisions

1. **IChatHubService abstraction** - The Application layer defines `IChatHubService` interface to abstract SignalR broadcasting. This allows services to broadcast events without depending on SignalR infrastructure, keeping the Application layer clean.

2. **ChatHubService in API layer** - The implementation lives in the API layer because it depends on `IHubContext<ChatHub>`, which is SignalR-specific. This is acceptable because the API layer already references Infrastructure.

3. **JWT via query string for WebSocket** - SignalR WebSocket connections can't use HTTP headers, so the JWT token is passed via `?access_token=` query parameter. The `OnMessageReceived` event handler extracts this token.

4. **Group naming convention** - SignalR groups are named `group-{groupId}` to avoid conflicts with other potential group types.

5. **Automatic broadcasting** - Services automatically broadcast events when state changes (message sent, member added, etc.), so clients receive real-time updates without polling.

### Client Usage Example

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat?access_token=" + accessToken)
  .withAutomaticReconnect()
  .build();

// Subscribe to events
connection.on("MessageReceived", (message) => {
  console.log("New message:", message);
});

connection.on("MemberAdded", (event) => {
  console.log("Member joined:", event.member);
});

connection.on("UserTyping", (event) => {
  console.log(event.displayName + " is typing...");
});

// Start connection
await connection.start();

// Join a group
await connection.invoke("JoinGroup", groupId);

// Start typing indicator
await connection.invoke("StartTyping", groupId);
```

---

## What's Next: Session 10

### Option A: AI Integration Preparation

Prepare for AI service integration:

- Create AI service HTTP client interface
- Define request/response contracts for Python AI service
- Implement AI message sending flow (detect @mention, call AI service)
- Store AI response metadata in `ai_response_metadata` table

### Option B: Read Status & Message Enhancements

Enhance messaging features:

- Add `message_reads` table for read tracking
- Implement cursor-based pagination (more efficient for real-time)
- Add unread message count endpoint

### Option C: Online Presence

Add user presence tracking:

- Track online/offline status per group
- Broadcast `UserOnline` / `UserOffline` events
- Store last seen timestamps

---

## Commands Reference

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run MessageService tests
dotnet test --filter "FullyQualifiedName~MessageService"

# Run GroupMemberService tests
dotnet test --filter "FullyQualifiedName~GroupMemberService"

# Start PostgreSQL
docker compose up -d

# Run the API
dotnet run --project src/AiGroupChat.API
```
