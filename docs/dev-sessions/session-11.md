# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 11: SignalR Phase 2 - Event Refinement & Test Coverage

**Date:** December 9, 2025

### Completed

#### 1. Event DTO Refactoring

Renamed and enriched SignalR event DTOs for better clarity and frontend usability.

**Renamed Events:**

| Old Name             | New Name            | Reason                                     |
| -------------------- | ------------------- | ------------------------------------------ |
| `MemberAddedEvent`   | `MemberJoinedEvent` | "Joined" is clearer than "Added" for users |
| `MemberRemovedEvent` | `MemberLeftEvent`   | "Left" covers both removal and leaving     |

**New DTO Created:**

- `UserStoppedTypingEvent.cs` - Previously was an inline anonymous object, now a proper DTO

**Enriched Event Fields:**

| Event                    | New Fields Added                    | Purpose                           |
| ------------------------ | ----------------------------------- | --------------------------------- |
| `MemberJoinedEvent`      | `DisplayName`                       | Show who joined without lookup    |
| `MemberLeftEvent`        | `DisplayName`                       | Show who left without lookup      |
| `MemberRoleChangedEvent` | `DisplayName`, `OldRole`, `NewRole` | Full context for role changes     |
| `AiSettingsChangedEvent` | `ChangedByName`, `ChangedAt`        | Audit trail for AI toggle changes |

#### 2. DTO Folder Reorganization

Reorganized SignalR DTOs into two subfolders for clarity:

```
DTOs/SignalR/
├── GroupChannel/           # Events sent to group-{groupId}
│   ├── AiSettingsChangedEvent.cs
│   ├── MemberJoinedEvent.cs
│   ├── MemberLeftEvent.cs
│   ├── MemberRoleChangedEvent.cs
│   ├── UserTypingEvent.cs
│   └── UserStoppedTypingEvent.cs
│
└── PersonalChannel/        # Events sent to user-{userId}
    ├── AddedToGroupEvent.cs
    ├── GroupActivityEvent.cs
    ├── NewMessageNotificationEvent.cs
    ├── RemovedFromGroupEvent.cs
    ├── RoleChangedEvent.cs
    ├── UserOfflineEvent.cs
    └── UserOnlineEvent.cs
```

#### 3. Service Updates

**GroupService:**

- `UpdateAiSettingsAsync` now fetches user to populate `ChangedByName`
- Added `IUserRepository` dependency

**GroupMemberService:**

- `AddMemberAsync` broadcasts `MemberJoinedEvent` with `DisplayName`
- `RemoveMemberAsync` broadcasts `MemberLeftEvent` with `DisplayName`
- `UpdateMemberRoleAsync` broadcasts `MemberRoleChangedEvent` with `DisplayName`, `OldRole`, `NewRole`

**ChatHubService:**

- Updated `BroadcastMemberJoinedAsync` (renamed from `BroadcastMemberAddedAsync`)
- Updated `BroadcastMemberLeftAsync` (renamed from `BroadcastMemberRemovedAsync`)
- Updated `BroadcastMemberRoleChangedAsync` to accept full event DTO
- Updated `BroadcastUserStoppedTypingAsync` to use new DTO

#### 4. Unit Test Fixes

Fixed compilation errors caused by the DTO refactoring:

**GroupServiceTestBase.cs:**

- Added `IUserRepository` mock (required for `ChangedByName` field)

**MemberBroadcastTests.cs:**

- Updated to use `MemberJoinedEvent` instead of `MemberAddedEvent`
- Updated to use `MemberLeftEvent` instead of `MemberRemovedEvent`
- Updated role change test to verify new fields (`OldRole`, `NewRole`, `DisplayName`)
- Added User mocks for owner/member to populate DisplayName fields

**UpdateAiSettingsBroadcastTests.cs:**

- Added UserRepository setup in constructor
- Updated assertions to verify `ChangedByName` and `ChangedAt` fields

#### 5. New Unit Tests

**SendMessagePersonalNotificationTests.cs** (4 tests):

| Test                                                                   | Description                                               |
| ---------------------------------------------------------------------- | --------------------------------------------------------- |
| `SendMessageAsync_SendsGroupActivityToOtherMembers`                    | Verifies GroupActivityEvent sent to all except sender     |
| `SendMessageAsync_SendsNewMessageNotificationToOtherMembers`           | Verifies notification with GroupName, SenderName, Preview |
| `SendMessageAsync_TruncatesLongMessagePreview`                         | Verifies 50-char truncation with "..." suffix             |
| `SendMessageAsync_DoesNotSendPersonalNotifications_WhenNoOtherMembers` | Edge case: solo member gets no notifications              |

**PersonalChannelNotificationTests.cs** (4 tests):

| Test                                                  | Description                                                    |
| ----------------------------------------------------- | -------------------------------------------------------------- |
| `AddMemberAsync_SendsAddedToGroupNotification`        | Verifies AddedToGroupEvent with GroupName, AddedByName, Role   |
| `RemoveMemberAsync_SendsRemovedFromGroupNotification` | Verifies RemovedFromGroupEvent with GroupName                  |
| `UpdateMemberRoleAsync_SendsRoleChangedNotification`  | Verifies RoleChangedEvent with OldRole, NewRole, ChangedByName |
| `LeaveGroupAsync_DoesNotSendPersonalNotification`     | Voluntary leave doesn't notify self                            |

#### 6. Test Coverage Summary

| Service            | Before  | After   | New Tests Added                               |
| ------------------ | ------- | ------- | --------------------------------------------- |
| MessageService     | 10      | 16      | +6 (broadcast + personal notifications)       |
| GroupMemberService | 31      | 40      | +9 (broadcast fixes + personal notifications) |
| GroupService       | 22      | 24      | +2 (AI settings broadcast)                    |
| ConnectionTracker  | 12      | 12      | (no changes)                                  |
| **Total**          | **107** | **124** | **+17 tests**                                 |

#### 7. README Documentation Updates

| File                                     | Changes                                             |
| ---------------------------------------- | --------------------------------------------------- |
| `tests/.../MessageService/README.md`     | Added broadcast and personal notification test docs |
| `tests/.../GroupMemberService/README.md` | Added SignalR event tables, updated test counts     |
| `tests/.../GroupService/README.md`       | Added AI settings broadcast section                 |
| `src/AiGroupChat.Application/README.md`  | Complete overhaul with SignalR DTO documentation    |
| `src/AiGroupChat.API/README.md`          | Fixed event names, added personal channel events    |

### Architecture Decisions

1. **Two-channel SignalR architecture** - Group channel (`group-{groupId}`) for active chat viewers, personal channel (`user-{userId}`) for notifications. This scales better than subscribing users to all their groups on connect.

2. **Human-readable fields in events** - Events include `DisplayName`, `GroupName`, etc. so the frontend doesn't need to make additional API calls to display notifications.

3. **Separate DTOs for each channel type** - Group channel events in `DTOs/SignalR/GroupChannel/`, personal channel events in `DTOs/SignalR/PersonalChannel/`. This makes it clear which events go where.

4. **Message preview truncation** - `NewMessageNotificationEvent` truncates content to 50 characters with "..." for push notification compatibility.

5. **No self-notifications on voluntary leave** - When a user leaves a group themselves, they don't receive a `RemovedFromGroup` notification (they already know).

### Files Created

```
src/AiGroupChat.Application/DTOs/SignalR/GroupChannel/
├── UserStoppedTypingEvent.cs                    # New DTO

tests/AiGroupChat.UnitTests/Services/MessageService/
├── SendMessagePersonalNotificationTests.cs      # 4 tests

tests/AiGroupChat.UnitTests/Services/GroupMemberService/
├── PersonalChannelNotificationTests.cs          # 4 tests
```

### Files Modified

```
src/AiGroupChat.Application/
├── DTOs/SignalR/GroupChannel/
│   ├── MemberJoinedEvent.cs                     # Renamed, added DisplayName
│   ├── MemberLeftEvent.cs                       # Renamed, added DisplayName
│   ├── MemberRoleChangedEvent.cs                # Added DisplayName, OldRole, NewRole
│   └── AiSettingsChangedEvent.cs                # Added ChangedByName, ChangedAt
├── Interfaces/IChatHubService.cs                # Updated method signatures
├── Services/GroupService.cs                     # Added IUserRepository, populate ChangedByName
└── Services/GroupMemberService.cs               # Populate new event fields

src/AiGroupChat.API/
├── Services/ChatHubService.cs                   # Updated method names and signatures
└── README.md                                    # Updated SignalR documentation

tests/AiGroupChat.UnitTests/Services/
├── GroupService/
│   ├── GroupServiceTestBase.cs                  # Added UserRepositoryMock
│   ├── UpdateAiSettingsBroadcastTests.cs        # Fixed for new event fields
│   └── README.md                                # Updated
├── GroupMemberService/
│   ├── MemberBroadcastTests.cs                  # Fixed for renamed events
│   └── README.md                                # Updated
└── MessageService/
    └── README.md                                # Updated
```

---

## SignalR Event Reference (Updated)

### Group Channel Events (`group-{groupId}`)

| Event               | Payload                  | When Triggered            |
| ------------------- | ------------------------ | ------------------------- |
| `MessageReceived`   | `MessageResponse`        | New message sent in group |
| `MemberJoined`      | `MemberJoinedEvent`      | New member added to group |
| `MemberLeft`        | `MemberLeftEvent`        | Member removed or left    |
| `MemberRoleChanged` | `MemberRoleChangedEvent` | Member's role changed     |
| `AiSettingsChanged` | `AiSettingsChangedEvent` | AI monitoring toggled     |
| `UserTyping`        | `UserTypingEvent`        | User started typing       |
| `UserStoppedTyping` | `UserStoppedTypingEvent` | User stopped typing       |

### Personal Channel Events (`user-{userId}`)

| Event                    | Payload                       | When Triggered                 |
| ------------------------ | ----------------------------- | ------------------------------ |
| `GroupActivity`          | `GroupActivityEvent`          | Any activity in user's groups  |
| `NewMessageNotification` | `NewMessageNotificationEvent` | New message (for push notifs)  |
| `AddedToGroup`           | `AddedToGroupEvent`           | User was added to a group      |
| `RemovedFromGroup`       | `RemovedFromGroupEvent`       | User was removed from a group  |
| `RoleChanged`            | `RoleChangedEvent`            | User's role changed in a group |
| `UserOnline`             | `UserOnlineEvent`             | Shared user came online        |
| `UserOffline`            | `UserOfflineEvent`            | Shared user went offline       |

---

## What's Next: Session 12

### Option A: AI Integration

Connect to the Python AI service:

- Create `IAiClientService` interface for HTTP communication
- Define request/response contracts matching Python service
- Implement @mention detection in `MessageService`
- Store AI response metadata in `ai_response_metadata` table
- Broadcast AI typing indicators and responses

### Option B: Integration Tests

Add integration tests for the full SignalR flow:

- Test hub connection with JWT authentication
- Test group join/leave operations
- Test event broadcasting end-to-end
- Test personal channel delivery

### Option C: Message Enhancements

Improve messaging features:

- Add message editing (with `EditedAt` timestamp)
- Add message deletion (soft delete)
- Add cursor-based pagination for better real-time sync
- Add unread message count tracking

---

## Commands Reference

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific service tests
dotnet test --filter "FullyQualifiedName~MessageService"
dotnet test --filter "FullyQualifiedName~GroupMemberService"
dotnet test --filter "FullyQualifiedName~GroupService"

# Run only SignalR-related tests
dotnet test --filter "FullyQualifiedName~Broadcast"
dotnet test --filter "FullyQualifiedName~PersonalChannel"
dotnet test --filter "FullyQualifiedName~ConnectionTracker"

# Start PostgreSQL
docker compose up -d

# Run the API
dotnet run --project src/AiGroupChat.API

# Access Scalar docs
open http://localhost:5126/scalar/v1
```
