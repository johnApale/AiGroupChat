# SignalR Real-Time Messaging Specification

## Overview

This document specifies the SignalR implementation for real-time messaging, notifications, and presence tracking in the AI Group Chat application.

## Architecture

### Single Hub, Two Channel Types

```
┌─────────────────────────────────────────────────────────────────┐
│                         ChatHub                                  │
│                    Endpoint: /hubs/chat                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   Personal Channel: "user-{userId}"                              │
│   ─────────────────────────────────                              │
│   • Joined: Automatically on connect                             │
│   • Left: Automatically on disconnect                            │
│   • Purpose: Notifications, presence, home page updates          │
│                                                                  │
│   Group Channel: "group-{groupId}"                               │
│   ────────────────────────────────                               │
│   • Joined: Manually when viewing a chat                         │
│   • Left: Manually when leaving a chat view                      │
│   • Purpose: Full message content, typing indicators             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Why Two Channel Types?

| Concern            | Personal Channel            | Group Channel               |
| ------------------ | --------------------------- | --------------------------- |
| Scalability        | 1 subscription per user     | 1 per active chat           |
| User in 100 groups | Still 1 subscription        | Only active chat subscribed |
| Payload size       | Lightweight (notifications) | Full content (messages)     |
| Always connected   | Yes                         | Only when viewing           |

---

## Connection Lifecycle

### Connect Flow

```
1. User authenticates via REST API → receives JWT
2. Client establishes SignalR connection with JWT
3. OnConnectedAsync() triggers:
   a. Validate JWT, extract userId
   b. Add connection to tracker (userId → connectionId)
   c. Join personal channel: "user-{userId}"
   d. If first connection for user:
      - Mark user as online
      - Broadcast UserOnline to relevant users
```

### Disconnect Flow

```
1. Client disconnects (close tab, network loss, logout)
2. OnDisconnectedAsync() triggers:
   a. Remove connection from tracker
   b. If no more connections for user:
      - Mark user as offline
      - Broadcast UserOffline to relevant users
   c. SignalR auto-removes from all groups
```

### Multiple Connections (Same User)

A user may have multiple tabs/devices. The connection tracker handles this:

```
User "john-123" connections:
├── connection-abc (Browser Tab 1)
├── connection-def (Browser Tab 2)
└── connection-ghi (Mobile App)

Tab 1 closes → Remove connection-abc
→ User still has 2 connections → STAY ONLINE

All tabs close → Remove all connections
→ User has 0 connections → BROADCAST OFFLINE
```

---

## Connection Tracker

### Interface

```csharp
public interface IConnectionTracker
{
    /// <summary>
    /// Adds a connection for a user. Returns true if this is the user's first connection.
    /// </summary>
    bool AddConnection(string userId, string connectionId);

    /// <summary>
    /// Removes a connection for a user. Returns true if user has no more connections.
    /// </summary>
    bool RemoveConnection(string userId, string connectionId);

    /// <summary>
    /// Checks if a user has at least one active connection.
    /// </summary>
    bool IsUserOnline(string userId);

    /// <summary>
    /// Gets online status for multiple users. Returns set of online user IDs.
    /// </summary>
    IReadOnlySet<string> GetOnlineUsers(IEnumerable<string> userIds);

    /// <summary>
    /// Gets all connection IDs for a user.
    /// </summary>
    IReadOnlyList<string> GetConnections(string userId);
}
```

### Implementation Notes

- Use `ConcurrentDictionary<string, HashSet<string>>` for thread safety
- Key: userId, Value: set of connectionIds
- In-memory for MVP (single server)
- Future: Redis backplane for multi-server deployment

---

## Events Specification

### Personal Channel Events (`user-{userId}`)

#### GroupActivityEvent

Sent when any activity occurs in a group the user belongs to. Used for home page list reordering.

```csharp
public class GroupActivityEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;  // "NewMessage", "MemberJoined", etc.
    public DateTime Timestamp { get; set; }
    public string? Preview { get; set; }                      // "John: Hey everyone..."
    public string? ActorName { get; set; }                    // Who triggered the activity
}
```

**Triggered by:**

- Message sent to group
- Member added/removed
- AI settings changed

---

#### NewMessageNotificationEvent

Sent when a new message is posted in any group the user belongs to. Used for notification badge/drawer.

```csharp
public class NewMessageNotificationEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid MessageId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;       // Truncated message content
    public DateTime SentAt { get; set; }
}
```

**Triggered by:**

- Message sent to group (excluding sender)

---

#### AddedToGroupEvent

Sent when user is added to a new group.

```csharp
public class AddedToGroupEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string AddedByName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;          // "Member", "Admin"
    public DateTime AddedAt { get; set; }
}
```

**Triggered by:**

- GroupMemberService.AddMemberAsync()

**Additional action:**

- Auto-subscribe user to group channel if connected

---

#### RemovedFromGroupEvent

Sent when user is removed from a group.

```csharp
public class RemovedFromGroupEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime RemovedAt { get; set; }
}
```

**Triggered by:**

- GroupMemberService.RemoveMemberAsync()

**Additional action:**

- Auto-unsubscribe user from group channel

---

#### RoleChangedEvent

Sent when user's role in a group changes.

```csharp
public class RoleChangedEvent
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string OldRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
```

**Triggered by:**

- GroupMemberService.UpdateMemberRoleAsync()

---

#### UserOnlineEvent

Sent when a user comes online (first connection established).

```csharp
public class UserOnlineEvent
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime OnlineAt { get; set; }
}
```

**Triggered by:**

- ChatHub.OnConnectedAsync() when first connection for user

**Recipients:**

- All users who share at least one group with this user

---

#### UserOfflineEvent

Sent when a user goes offline (last connection closed).

```csharp
public class UserOfflineEvent
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime OfflineAt { get; set; }
}
```

**Triggered by:**

- ChatHub.OnDisconnectedAsync() when last connection for user

**Recipients:**

- All users who share at least one group with this user

---

### Group Channel Events (`group-{groupId}`)

#### MessageReceivedEvent

Full message content for active chat view.

```csharp
public class MessageReceivedEvent
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string? SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsAiGenerated { get; set; }
    public Guid? AiProviderId { get; set; }
}
```

**Triggered by:**

- MessageService.SendMessageAsync()

**Note:** Currently using `MessageResponse` DTO. Will be renamed to `MessageReceivedEvent` for consistency.

---

#### UserTypingEvent

Sent when a user starts typing.

```csharp
public class UserTypingEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
```

**Triggered by:**

- Client calls ChatHub.StartTyping(groupId)

---

#### UserStoppedTypingEvent

Sent when a user stops typing.

```csharp
public class UserStoppedTypingEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
```

**Triggered by:**

- Client calls ChatHub.StopTyping(groupId)

**Note:** Currently sends `(Guid groupId, string userId)` as separate parameters. Will be updated to use this DTO.

---

#### MemberJoinedEvent

Sent when a new member joins the group.

```csharp
public class MemberJoinedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
```

**Triggered by:**

- GroupMemberService.AddMemberAsync()

**Note:** Currently using `MemberAddedEvent`. Will be renamed to `MemberJoinedEvent` for clarity.

---

#### MemberLeftEvent

Sent when a member leaves or is removed from the group.

```csharp
public class MemberLeftEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime LeftAt { get; set; }
}
```

**Triggered by:**

- GroupMemberService.RemoveMemberAsync()

**Note:** Currently using `MemberRemovedEvent`. Will be renamed to `MemberLeftEvent` for clarity.

---

#### MemberRoleChangedEvent

Sent when a member's role changes.

```csharp
public class MemberRoleChangedEvent
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string OldRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
}
```

**Triggered by:**

- GroupMemberService.UpdateMemberRoleAsync()

---

#### AiSettingsChangedEvent

Sent when AI monitoring is toggled.

```csharp
public class AiSettingsChangedEvent
{
    public Guid GroupId { get; set; }
    public bool AiMonitoringEnabled { get; set; }
    public Guid? AiProviderId { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
```

**Triggered by:**

- GroupService.UpdateAiSettingsAsync()

**Note:** Already implemented but may need `ChangedByName` and `ChangedAt` fields added.

---

## Hub Methods (Client → Server)

### JoinGroup

Subscribe to a group channel for full message events.

```csharp
public async Task JoinGroup(Guid groupId)
```

**Validation:**

- User must be authenticated
- User must be a member of the group

**Action:**

- Add connection to SignalR group `group-{groupId}`

---

### LeaveGroup

Unsubscribe from a group channel.

```csharp
public async Task LeaveGroup(Guid groupId)
```

**Action:**

- Remove connection from SignalR group `group-{groupId}`

---

### StartTyping

Broadcast typing indicator to group.

```csharp
public async Task StartTyping(Guid groupId)
```

**Validation:**

- User must be a member of the group

**Action:**

- Broadcast `UserTyping` to `group-{groupId}` (excluding caller)

---

### StopTyping

Stop typing indicator.

```csharp
public async Task StopTyping(Guid groupId)
```

**Action:**

- Broadcast `UserStoppedTyping` to `group-{groupId}` (excluding caller)

---

## Service Interfaces

### IChatHubService

```csharp
public interface IChatHubService
{
    // ============================================
    // Personal Channel Notifications (user-{userId})
    // ============================================

    /// <summary>
    /// Sends group activity to all members' personal channels (for home page reordering).
    /// </summary>
    Task SendGroupActivityAsync(Guid groupId, GroupActivityEvent activity, CancellationToken ct = default);

    /// <summary>
    /// Sends new message notification to all members' personal channels (for notification badge).
    /// Excludes the sender.
    /// </summary>
    Task SendNewMessageNotificationAsync(Guid groupId, string excludeUserId, NewMessageNotificationEvent notification, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user they were added to a group.
    /// </summary>
    Task SendAddedToGroupAsync(string userId, AddedToGroupEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user they were removed from a group.
    /// </summary>
    Task SendRemovedFromGroupAsync(string userId, RemovedFromGroupEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user their role changed in a group.
    /// </summary>
    Task SendRoleChangedAsync(string userId, RoleChangedEvent evt, CancellationToken ct = default);

    // ============================================
    // Presence Events (user-{userId})
    // ============================================

    /// <summary>
    /// Broadcasts user online status to all users who share groups with them.
    /// </summary>
    Task BroadcastUserOnlineAsync(string userId, UserOnlineEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Broadcasts user offline status to all users who share groups with them.
    /// </summary>
    Task BroadcastUserOfflineAsync(string userId, UserOfflineEvent evt, CancellationToken ct = default);

    // ============================================
    // Group Channel Broadcasts (group-{groupId})
    // ============================================

    /// <summary>
    /// Broadcasts a new message to all users viewing the group chat.
    /// </summary>
    Task BroadcastMessageAsync(Guid groupId, MessageResponse message, CancellationToken ct = default);

    /// <summary>
    /// Broadcasts that a member joined the group.
    /// </summary>
    Task BroadcastMemberJoinedAsync(Guid groupId, MemberJoinedEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Broadcasts that a member left/was removed from the group.
    /// </summary>
    Task BroadcastMemberLeftAsync(Guid groupId, MemberLeftEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Broadcasts that a member's role changed.
    /// </summary>
    Task BroadcastMemberRoleChangedAsync(Guid groupId, MemberRoleChangedEvent evt, CancellationToken ct = default);

    /// <summary>
    /// Broadcasts that AI settings changed for the group.
    /// </summary>
    Task BroadcastAiSettingsChangedAsync(Guid groupId, AiSettingsChangedEvent evt, CancellationToken ct = default);

    // ============================================
    // Channel Management
    // ============================================

    /// <summary>
    /// Subscribes a user to a group channel (used when user is added to group while connected).
    /// </summary>
    Task SubscribeUserToGroupAsync(string userId, Guid groupId, CancellationToken ct = default);

    /// <summary>
    /// Unsubscribes a user from a group channel (used when user is removed from group).
    /// </summary>
    Task UnsubscribeUserFromGroupAsync(string userId, Guid groupId, CancellationToken ct = default);
}
```

---

## Message Flow Examples

### Example 1: New Message Sent

```
User "John" sends message to Group A
│
├─► MessageService.SendMessageAsync()
│   │
│   ├─► Save message to database
│   │
│   ├─► Get all members of Group A: [John, Sarah, Mike]
│   │
│   ├─► For each member (except sender):
│   │   └─► Send to "user-{memberId}":
│   │       • GroupActivity { groupId, "NewMessage", preview }
│   │       • NewMessageNotification { groupId, messageId, preview }
│   │
│   └─► Send to "group-{groupA.Id}":
│       • MessageReceived { full message content }
│
└─► Done
```

### Example 2: User Opens App

```
User "Sarah" opens app, connects to SignalR
│
├─► OnConnectedAsync()
│   │
│   ├─► Extract userId from JWT
│   │
│   ├─► connectionTracker.AddConnection(userId, connectionId)
│   │   └─► Returns true (first connection)
│   │
│   ├─► Join "user-{sarah.id}" (personal channel)
│   │
│   ├─► Get users who share groups with Sarah: [John, Mike, Lisa]
│   │
│   └─► For each related user:
│       └─► Send to "user-{relatedUserId}":
│           • UserOnline { userId, displayName }
│
└─► Sarah is now:
    • Receiving notifications on personal channel
    • Visible as "online" to John, Mike, Lisa
```

### Example 3: User Opens Specific Chat

```
User "Sarah" opens Group A chat
│
├─► Client calls: hub.invoke("JoinGroup", groupA.id)
│   │
│   ├─► Validate Sarah is member of Group A
│   │
│   └─► Add connection to "group-{groupA.id}"
│
└─► Sarah is now:
    • Receiving full messages on group channel
    • Seeing typing indicators
    • Still receiving notifications on personal channel
```

### Example 4: User Added to New Group While Online

```
Admin adds "Sarah" to Group B (Sarah already connected)
│
├─► GroupMemberService.AddMemberAsync()
│   │
│   ├─► Save membership to database
│   │
│   ├─► Send to "user-{sarah.id}":
│   │   • AddedToGroup { groupId, groupName, addedBy }
│   │
│   ├─► Auto-subscribe Sarah to Group B notifications:
│   │   └─► chatHubService.SubscribeUserToGroupAsync(sarah.id, groupB.id)
│   │       └─► For each of Sarah's connections:
│   │           └─► Add to "group-{groupB.id}" (optional, for immediate updates)
│   │
│   └─► Send to "group-{groupB.id}":
│       • MemberJoined { userId, displayName }
│
└─► Sarah immediately:
    • Sees toast: "You were added to Group B"
    • Group B appears in her group list
    • Can receive messages from Group B
```

---

## Frontend Integration

### Connection Setup

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => getAccessToken(),
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### Event Handlers

```typescript
// ============================================
// Personal Channel Events (always received)
// ============================================

connection.on("GroupActivity", (event: GroupActivityEvent) => {
  reorderGroupList(event.groupId, event.timestamp);
  updateGroupPreview(event.groupId, event.preview);
});

connection.on(
  "NewMessageNotification",
  (event: NewMessageNotificationEvent) => {
    incrementNotificationBadge();
    addToNotificationDrawer(event);
    if (currentGroupId !== event.groupId) {
      incrementGroupUnread(event.groupId);
    }
  }
);

connection.on("AddedToGroup", (event: AddedToGroupEvent) => {
  showToast(`Added to ${event.groupName}`);
  refreshGroupList();
});

connection.on("RemovedFromGroup", (event: RemovedFromGroupEvent) => {
  showToast(`Removed from ${event.groupName}`);
  removeGroupFromList(event.groupId);
  if (currentGroupId === event.groupId) {
    navigateToHome();
  }
});

connection.on("RoleChanged", (event: RoleChangedEvent) => {
  showToast(`Your role in ${event.groupName} changed to ${event.newRole}`);
});

connection.on("UserOnline", (event: UserOnlineEvent) => {
  setUserOnline(event.userId);
});

connection.on("UserOffline", (event: UserOfflineEvent) => {
  setUserOffline(event.userId);
});

// ============================================
// Group Channel Events (only when viewing that group)
// ============================================

connection.on("MessageReceived", (message: MessageReceivedEvent) => {
  appendMessage(message);
});

connection.on("UserTyping", (event: UserTypingEvent) => {
  showTypingIndicator(event.userId, event.displayName);
});

connection.on("UserStoppedTyping", (event: UserStoppedTypingEvent) => {
  hideTypingIndicator(event.userId);
});

connection.on("MemberJoined", (event: MemberJoinedEvent) => {
  addMemberToList(event);
});

connection.on("MemberLeft", (event: MemberLeftEvent) => {
  removeMemberFromList(event.userId);
});

connection.on("MemberRoleChanged", (event: MemberRoleChangedEvent) => {
  updateMemberRole(event.userId, event.newRole);
});

connection.on("AiSettingsChanged", (event: AiSettingsChangedEvent) => {
  updateAiSettings(event.aiMonitoringEnabled, event.aiProviderId);
});
```

### Joining/Leaving Group Channels

```typescript
// When user opens a chat
async function openChat(groupId: string) {
  await connection.invoke("JoinGroup", groupId);
  setCurrentGroupId(groupId);
  clearGroupUnread(groupId);
}

// When user leaves a chat
async function leaveChat(groupId: string) {
  await connection.invoke("LeaveGroup", groupId);
  setCurrentGroupId(null);
}

// Typing indicators
let typingTimeout: NodeJS.Timeout;

function onMessageInput() {
  connection.invoke("StartTyping", currentGroupId);

  clearTimeout(typingTimeout);
  typingTimeout = setTimeout(() => {
    connection.invoke("StopTyping", currentGroupId);
  }, 3000);
}
```

### TypeScript Types

```typescript
// Personal Channel Events
interface GroupActivityEvent {
  groupId: string;
  groupName: string;
  activityType: string;
  timestamp: string;
  preview?: string;
  actorName?: string;
}

interface NewMessageNotificationEvent {
  groupId: string;
  groupName: string;
  messageId: string;
  senderName: string;
  preview: string;
  sentAt: string;
}

interface AddedToGroupEvent {
  groupId: string;
  groupName: string;
  addedByName: string;
  role: string;
  addedAt: string;
}

interface RemovedFromGroupEvent {
  groupId: string;
  groupName: string;
  removedAt: string;
}

interface RoleChangedEvent {
  groupId: string;
  groupName: string;
  oldRole: string;
  newRole: string;
  changedByName: string;
  changedAt: string;
}

interface UserOnlineEvent {
  userId: string;
  displayName: string;
  onlineAt: string;
}

interface UserOfflineEvent {
  userId: string;
  displayName: string;
  offlineAt: string;
}

// Group Channel Events
interface MessageReceivedEvent {
  id: string;
  groupId: string;
  senderId?: string;
  senderName: string;
  content: string;
  sentAt: string;
  isAiGenerated: boolean;
  aiProviderId?: string;
}

interface UserTypingEvent {
  groupId: string;
  userId: string;
  displayName: string;
}

interface UserStoppedTypingEvent {
  groupId: string;
  userId: string;
}

interface MemberJoinedEvent {
  groupId: string;
  userId: string;
  displayName: string;
  role: string;
  joinedAt: string;
}

interface MemberLeftEvent {
  groupId: string;
  userId: string;
  displayName: string;
  leftAt: string;
}

interface MemberRoleChangedEvent {
  groupId: string;
  userId: string;
  displayName: string;
  oldRole: string;
  newRole: string;
}

interface AiSettingsChangedEvent {
  groupId: string;
  aiMonitoringEnabled: boolean;
  aiProviderId?: string;
  changedByName: string;
  changedAt: string;
}
```

---

## File Structure

```
src/
├── AiGroupChat.Application/
│   ├── DTOs/
│   │   └── SignalR/
│   │       ├── Personal Channel Events/
│   │       │   ├── GroupActivityEvent.cs
│   │       │   ├── NewMessageNotificationEvent.cs
│   │       │   ├── AddedToGroupEvent.cs
│   │       │   ├── RemovedFromGroupEvent.cs
│   │       │   ├── RoleChangedEvent.cs
│   │       │   ├── UserOnlineEvent.cs
│   │       │   └── UserOfflineEvent.cs
│   │       │
│   │       └── Group Channel Events/
│   │           ├── MessageReceivedEvent.cs      (or keep using MessageResponse)
│   │           ├── UserTypingEvent.cs           ✅ Exists
│   │           ├── UserStoppedTypingEvent.cs    (currently inline params)
│   │           ├── MemberJoinedEvent.cs         (rename from MemberAddedEvent)
│   │           ├── MemberLeftEvent.cs           (rename from MemberRemovedEvent)
│   │           ├── MemberRoleChangedEvent.cs    ✅ Exists
│   │           └── AiSettingsChangedEvent.cs    ✅ Exists
│   │
│   └── Interfaces/
│       ├── IChatHubService.cs                   (update with new methods)
│       └── IConnectionTracker.cs                NEW
│
├── AiGroupChat.API/
│   ├── Hubs/
│   │   └── ChatHub.cs                           (update OnConnected/OnDisconnected)
│   └── Services/
│       ├── ChatHubService.cs                    (update with new methods)
│       └── ConnectionTracker.cs                 NEW
│
└── AiGroupChat.Infrastructure/
    └── Repositories/
        └── GroupMemberRepository.cs             (add GetUsersWhoShareGroupsWithAsync)
```

---

## Implementation Phases

### Current State (Session 9 Complete)

The following group channel events are already implemented and working:

| Event               | Channel           | DTO                      | Status         |
| ------------------- | ----------------- | ------------------------ | -------------- |
| `MessageReceived`   | `group-{groupId}` | `MessageResponse`        | ✅ Implemented |
| `UserTyping`        | `group-{groupId}` | `UserTypingEvent`        | ✅ Implemented |
| `UserStoppedTyping` | `group-{groupId}` | `(Guid, string)` params  | ✅ Implemented |
| `MemberAdded`       | `group-{groupId}` | `MemberAddedEvent`       | ✅ Implemented |
| `MemberRemoved`     | `group-{groupId}` | `MemberRemovedEvent`     | ✅ Implemented |
| `MemberRoleChanged` | `group-{groupId}` | `MemberRoleChangedEvent` | ✅ Implemented |
| `AiSettingsChanged` | `group-{groupId}` | `AiSettingsChangedEvent` | ✅ Implemented |

---

### Phase 1: Personal Channel + Presence

#### What We Keep (No Changes)

All 7 existing group channel events remain unchanged. They work correctly for users actively viewing a chat.

#### What We Add (New)

| Component                     | Location                 | Purpose                                 |
| ----------------------------- | ------------------------ | --------------------------------------- |
| `IConnectionTracker`          | Application/Interfaces   | Interface for tracking user connections |
| `ConnectionTracker`           | API/Services             | In-memory implementation                |
| `GroupActivityEvent`          | Application/DTOs/SignalR | Home page list reordering               |
| `NewMessageNotificationEvent` | Application/DTOs/SignalR | Notification badge/drawer               |
| `AddedToGroupEvent`           | Application/DTOs/SignalR | Toast when added to group               |
| `RemovedFromGroupEvent`       | Application/DTOs/SignalR | Toast when removed from group           |
| `RoleChangedEvent`            | Application/DTOs/SignalR | Toast when role changes                 |
| `UserOnlineEvent`             | Application/DTOs/SignalR | Green bubble (online)                   |
| `UserOfflineEvent`            | Application/DTOs/SignalR | Gray bubble (offline)                   |

#### What We Modify

| Component                                    | Current Behavior                           | New Behavior                                                              |
| -------------------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------- |
| `ChatHub.OnConnectedAsync()`                 | Empty (just calls base)                    | Auto-join `user-{userId}`, broadcast `UserOnline`                         |
| `ChatHub.OnDisconnectedAsync()`              | Not overridden                             | Broadcast `UserOffline` when last connection closes                       |
| `IChatHubService`                            | 5 methods (group broadcasts only)          | Add 9 new methods (personal channel + presence + channel management)      |
| `ChatHubService`                             | Implements group broadcasts only           | Add personal channel broadcast implementations                            |
| `MessageService.SendMessageAsync()`          | Broadcasts `MessageReceived` to group only | Also send `GroupActivity` + `NewMessageNotification` to personal channels |
| `GroupMemberService.AddMemberAsync()`        | Broadcasts `MemberAdded` to group          | Also send `AddedToGroup` to user's personal channel                       |
| `GroupMemberService.RemoveMemberAsync()`     | Broadcasts `MemberRemoved` to group        | Also send `RemovedFromGroup` to user's personal channel                   |
| `GroupMemberService.UpdateMemberRoleAsync()` | Broadcasts `MemberRoleChanged` to group    | Also send `RoleChanged` to user's personal channel                        |
| `IGroupMemberRepository`                     | No method to find shared users             | Add `GetUsersWhoShareGroupsWithAsync()`                                   |
| `GroupMemberRepository`                      | No method to find shared users             | Implement `GetUsersWhoShareGroupsWithAsync()`                             |

#### New Repository Method

```csharp
// IGroupMemberRepository - needed to find users for presence broadcasts
Task<List<string>> GetUsersWhoShareGroupsWithAsync(string userId, CancellationToken ct = default);
```

#### Phase 1 Outcome

After Phase 1 completion:

- ✅ Home page updates in real-time when messages arrive in any group
- ✅ Notification badge increments for new messages
- ✅ Users see green/gray online indicators for other users
- ✅ Toast notifications when added/removed from groups or role changes
- ✅ All existing group channel functionality preserved

---

### Phase 2: Polish & Rename (Optional)

Rename existing DTOs for consistency:

| Current Name                        | New Name                 | Reason                        |
| ----------------------------------- | ------------------------ | ----------------------------- |
| `MemberAddedEvent`                  | `MemberJoinedEvent`      | Clearer naming                |
| `MemberRemovedEvent`                | `MemberLeftEvent`        | Clearer naming                |
| `UserStoppedTyping` (inline params) | `UserStoppedTypingEvent` | Consistency with other events |

Add missing fields to existing events:

| Event                    | Add Fields                   |
| ------------------------ | ---------------------------- |
| `AiSettingsChangedEvent` | `ChangedByName`, `ChangedAt` |
| `MemberRoleChangedEvent` | `DisplayName`, `OldRole`     |

---

## Testing Strategy

### Unit Tests

- `ConnectionTrackerTests` - Add/remove connections, multiple connections per user
- `ChatHubServiceTests` - Verify correct events sent to correct channels
- Mock `IHubContext<ChatHub>` to verify `SendAsync` calls

### Integration Tests

- Connect with valid JWT → verify personal channel joined
- Connect with invalid JWT → verify connection rejected
- Send message → verify notifications sent to all members
- User goes offline → verify event broadcast

---

## Security Considerations

1. **Authentication Required** - Hub requires valid JWT
2. **Membership Validation** - JoinGroup validates user is group member
3. **No Message Content in Notifications** - Preview is truncated, no sensitive data
4. **Personal Channels** - Users can only receive on their own channel
5. **Connection Tracking** - In-memory, cleared on server restart

---

## Scalability Notes

### Current (MVP - Single Server)

- In-memory connection tracker
- Direct SignalR groups
- No persistence of online status

### Future (Multi-Server)

- Redis backplane for SignalR
- Redis-based connection tracker
- Consider: Azure SignalR Service for managed scaling
