# SignalR Real-time Messaging Implementation

This document explains how SignalR was implemented in the AI Group Chat application, including architecture decisions, code placement, and frontend integration guide.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Code Structure](#code-structure)
4. [Implementation Details](#implementation-details)
5. [Event Flow](#event-flow)
6. [Frontend Integration Guide](#frontend-integration-guide)
7. [Testing](#testing)

---

## Overview

SignalR provides real-time WebSocket communication between the server and clients. In this application, it's used for:

- **Instant message delivery** - Messages appear immediately for all group members
- **Typing indicators** - Users see when others are typing
- **Live updates** - AI settings changes, member additions/removals are broadcast instantly

### Why SignalR?

- Built into ASP.NET Core - no external dependencies
- Automatic fallback to long-polling if WebSocket isn't available
- Built-in group management for broadcasting to specific users
- Easy JWT authentication integration

---

## Architecture

### High-Level Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND CLIENT                                │
│                                                                             │
│  1. Connect to /hubs/chat with JWT token                                    │
│  2. Call hub methods (JoinGroup, StartTyping, etc.)                         │
│  3. Listen for server events (MessageReceived, MemberAdded, etc.)           │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ WebSocket Connection
                                    │ (with JWT in query string)
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API LAYER                                      │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                           ChatHub                                    │   │
│  │  - Handles client connections                                        │   │
│  │  - Client→Server: JoinGroup, LeaveGroup, StartTyping, StopTyping    │   │
│  │  - Validates group membership before allowing operations             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        ChatHubService                                │   │
│  │  - Implements IChatHubService                                        │   │
│  │  - Uses IHubContext<ChatHub> to broadcast from services              │   │
│  │  - Server→Client: MessageReceived, MemberAdded, etc.                 │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ IChatHubService interface
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          APPLICATION LAYER                                  │
│                                                                             │
│  Services call IChatHubService to broadcast events:                         │
│                                                                             │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐     │
│  │ MessageService  │  │  GroupService   │  │  GroupMemberService     │     │
│  │                 │  │                 │  │                         │     │
│  │ SendMessage()   │  │ UpdateAi        │  │ AddMember()             │     │
│  │   → Broadcast   │  │ Settings()      │  │ RemoveMember()          │     │
│  │     Message     │  │   → Broadcast   │  │ UpdateRole()            │     │
│  │     Received    │  │     AiSettings  │  │   → Broadcast events    │     │
│  │                 │  │     Changed     │  │                         │     │
│  └─────────────────┘  └─────────────────┘  └─────────────────────────┘     │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer              | SignalR Responsibility                                                           |
| ------------------ | -------------------------------------------------------------------------------- |
| **API**            | Hub definition, WebSocket handling, broadcasting implementation                  |
| **Application**    | Interface definition (`IChatHubService`), calling broadcasts from business logic |
| **Infrastructure** | None - SignalR is handled entirely in API layer                                  |
| **Domain**         | None - no SignalR concerns                                                       |

---

## Code Structure

### Files Created

```
src/
├── AiGroupChat.API/
│   ├── Hubs/
│   │   └── ChatHub.cs              # SignalR hub - handles client connections
│   └── Services/
│       └── ChatHubService.cs       # Broadcasts events to clients
│
└── AiGroupChat.Application/
    ├── DTOs/
    │   └── SignalR/
    │       ├── AiSettingsChangedEvent.cs
    │       ├── MemberEvent.cs      # MemberAddedEvent, MemberRemovedEvent, MemberRoleChangedEvent
    │       └── UserTypingEvent.cs
    └── Interfaces/
        └── IChatHubService.cs      # Abstraction for broadcasting
```

### Why This Structure?

1. **`IChatHubService` in Application layer** - Services need to broadcast events but shouldn't depend on SignalR directly. The interface abstracts this, keeping Application layer clean.

2. **`ChatHub` in API layer** - The hub handles WebSocket connections, which is an API concern. It needs access to repositories to validate group membership.

3. **`ChatHubService` in API layer** - Implements `IChatHubService` using `IHubContext<ChatHub>`. This is SignalR-specific, so it lives alongside the hub.

4. **DTOs in Application layer** - Event payloads are DTOs, which belong in Application. They're used by both the service (to construct events) and the hub service (to broadcast them).

---

## Implementation Details

### 1. Hub Definition (`ChatHub.cs`)

```csharp
[Authorize]  // Requires JWT authentication
public class ChatHub : Hub
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;

    // Client → Server methods
    public async Task JoinGroup(Guid groupId)
    {
        // Validates user is a member before allowing subscription
        bool isMember = await _groupRepository.IsMemberAsync(groupId, GetUserId());
        if (!isMember) throw new HubException("You are not a member of this group.");

        // Add connection to SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{groupId}");
    }

    public async Task LeaveGroup(Guid groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{groupId}");
    }

    public async Task StartTyping(Guid groupId)
    {
        // Broadcast to others in group (not sender)
        await Clients.OthersInGroup($"group-{groupId}").SendAsync("UserTyping", typingEvent);
    }

    public async Task StopTyping(Guid groupId)
    {
        await Clients.OthersInGroup($"group-{groupId}").SendAsync("UserStoppedTyping", groupId, userId);
    }
}
```

### 2. Hub Service (`ChatHubService.cs`)

```csharp
public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    // Called by MessageService when a message is sent
    public async Task BroadcastMessageAsync(Guid groupId, MessageResponse message, CancellationToken ct)
    {
        await _hubContext.Clients
            .Group($"group-{groupId}")
            .SendAsync("MessageReceived", message, ct);
    }

    // Called by GroupService when AI settings change
    public async Task BroadcastAiSettingsChangedAsync(Guid groupId, AiSettingsChangedEvent settings, CancellationToken ct)
    {
        await _hubContext.Clients
            .Group($"group-{groupId}")
            .SendAsync("AiSettingsChanged", settings, ct);
    }

    // ... other broadcast methods
}
```

### 3. Service Integration (Example: `MessageService.cs`)

```csharp
public class MessageService : IMessageService
{
    private readonly IChatHubService _chatHubService;  // Injected

    public async Task<MessageResponse> SendMessageAsync(...)
    {
        // 1. Validate request
        // 2. Create message in database
        // 3. Map to response

        // 4. Broadcast to group members via SignalR
        await _chatHubService.BroadcastMessageAsync(groupId, response, cancellationToken);

        return response;
    }
}
```

### 4. JWT Authentication for WebSocket

WebSocket connections can't use HTTP headers, so the JWT token must be passed in the query string:

```csharp
// Program.cs
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        string? accessToken = context.Request.Query["access_token"];
        PathString path = context.HttpContext.Request.Path;

        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

---

## Event Flow

### Sending a Message

```
┌────────────┐     ┌────────────┐     ┌────────────┐     ┌────────────┐
│   Client   │     │    API     │     │  Service   │     │   SignalR  │
│  (Sender)  │     │ Controller │     │   Layer    │     │    Hub     │
└─────┬──────┘     └─────┬──────┘     └─────┬──────┘     └─────┬──────┘
      │                  │                  │                  │
      │ POST /api/groups │                  │                  │
      │ /:id/messages    │                  │                  │
      │─────────────────>│                  │                  │
      │                  │                  │                  │
      │                  │ SendMessageAsync │                  │
      │                  │─────────────────>│                  │
      │                  │                  │                  │
      │                  │                  │ Save to DB       │
      │                  │                  │────────────>     │
      │                  │                  │                  │
      │                  │                  │ BroadcastMessage │
      │                  │                  │─────────────────>│
      │                  │                  │                  │
      │                  │                  │                  │──────────────┐
      │                  │                  │                  │ Send to all  │
      │                  │                  │                  │ in group     │
      │                  │ MessageResponse  │                  │<─────────────┘
      │ 201 Created      │<─────────────────│                  │
      │<─────────────────│                  │                  │
      │                  │                  │                  │

┌────────────┐                                              ┌────────────┐
│   Client   │                                              │   SignalR  │
│ (Receiver) │                                              │    Hub     │
└─────┬──────┘                                              └─────┬──────┘
      │                                                           │
      │                    MessageReceived event                  │
      │<──────────────────────────────────────────────────────────│
      │                                                           │
```

### Typing Indicator

```
┌────────────┐     ┌────────────┐                          ┌────────────┐
│   Client   │     │  ChatHub   │                          │   Other    │
│  (Typing)  │     │            │                          │  Clients   │
└─────┬──────┘     └─────┬──────┘                          └─────┬──────┘
      │                  │                                       │
      │ StartTyping      │                                       │
      │ (groupId)        │                                       │
      │─────────────────>│                                       │
      │                  │                                       │
      │                  │ Validate membership                   │
      │                  │────────────>                          │
      │                  │                                       │
      │                  │           UserTyping event            │
      │                  │──────────────────────────────────────>│
      │                  │                                       │
      │                  │                                       │ Display
      │                  │                                       │ "User is
      │                  │                                       │ typing..."
      │                  │                                       │
      │ StopTyping       │                                       │
      │ (groupId)        │                                       │
      │─────────────────>│                                       │
      │                  │                                       │
      │                  │       UserStoppedTyping event         │
      │                  │──────────────────────────────────────>│
      │                  │                                       │
```

---

## Frontend Integration Guide

### 1. Install SignalR Client

```bash
# npm
npm install @microsoft/signalr

# yarn
yarn add @microsoft/signalr
```

### 2. Create Connection

```typescript
import * as signalR from "@microsoft/signalr";

// Create connection with JWT token
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE_URL}/hubs/chat?access_token=${accessToken}`, {
    skipNegotiation: true, // Optional: force WebSocket only
    transport: signalR.HttpTransportType.WebSockets,
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
  .configureLogging(signalR.LogLevel.Information)
  .build();
```

### 3. Handle Connection Events

```typescript
// Connection state changes
connection.onreconnecting((error) => {
  console.log("Reconnecting...", error);
  // Show reconnecting UI
});

connection.onreconnected((connectionId) => {
  console.log("Reconnected!", connectionId);
  // Re-join groups after reconnection
  rejoinGroups();
});

connection.onclose((error) => {
  console.log("Connection closed", error);
  // Handle disconnection
});
```

### 4. Subscribe to Events

```typescript
// New message received
connection.on("MessageReceived", (message: MessageResponse) => {
  console.log("New message:", message);
  // Add message to UI
  addMessageToChat(message);
});

// AI settings changed
connection.on("AiSettingsChanged", (event: AiSettingsChangedEvent) => {
  console.log("AI settings updated:", event);
  // Update group settings in state
  updateGroupAiSettings(event.groupId, event);
});

// Member added
connection.on("MemberAdded", (event: MemberAddedEvent) => {
  console.log("New member:", event.member);
  // Add member to group member list
  addGroupMember(event.groupId, event.member);
});

// Member removed
connection.on("MemberRemoved", (event: MemberRemovedEvent) => {
  console.log("Member removed:", event.userId);
  // Remove member from list, or redirect if it's current user
  removeGroupMember(event.groupId, event.userId);
});

// Member role changed
connection.on("MemberRoleChanged", (event: MemberRoleChangedEvent) => {
  console.log("Role changed:", event);
  // Update member role in UI
  updateMemberRole(event.groupId, event.userId, event.newRole);
});

// User typing
connection.on("UserTyping", (event: UserTypingEvent) => {
  console.log(`${event.displayName} is typing...`);
  // Show typing indicator
  showTypingIndicator(event.groupId, event);
});

// User stopped typing
connection.on("UserStoppedTyping", (groupId: string, userId: string) => {
  console.log("User stopped typing");
  // Hide typing indicator
  hideTypingIndicator(groupId, userId);
});
```

### 5. Start Connection and Join Groups

```typescript
async function startConnection() {
  try {
    await connection.start();
    console.log("SignalR connected");
  } catch (err) {
    console.error("SignalR connection error:", err);
    // Retry after delay
    setTimeout(startConnection, 5000);
  }
}

async function joinGroup(groupId: string) {
  try {
    await connection.invoke("JoinGroup", groupId);
    console.log(`Joined group ${groupId}`);
  } catch (err) {
    console.error("Failed to join group:", err);
  }
}

async function leaveGroup(groupId: string) {
  try {
    await connection.invoke("LeaveGroup", groupId);
    console.log(`Left group ${groupId}`);
  } catch (err) {
    console.error("Failed to leave group:", err);
  }
}
```

### 6. Send Typing Indicators

```typescript
let typingTimeout: NodeJS.Timeout | null = null;

function handleTyping(groupId: string) {
  // Send start typing
  connection.invoke("StartTyping", groupId).catch(console.error);

  // Clear existing timeout
  if (typingTimeout) {
    clearTimeout(typingTimeout);
  }

  // Auto-stop after 3 seconds of no typing
  typingTimeout = setTimeout(() => {
    connection.invoke("StopTyping", groupId).catch(console.error);
  }, 3000);
}

function handleStopTyping(groupId: string) {
  if (typingTimeout) {
    clearTimeout(typingTimeout);
  }
  connection.invoke("StopTyping", groupId).catch(console.error);
}
```

### 7. Full React Hook Example

```typescript
import { useEffect, useRef, useCallback } from "react";
import * as signalR from "@microsoft/signalr";

interface UseSignalROptions {
  accessToken: string;
  onMessageReceived?: (message: MessageResponse) => void;
  onMemberAdded?: (event: MemberAddedEvent) => void;
  onMemberRemoved?: (event: MemberRemovedEvent) => void;
  onUserTyping?: (event: UserTypingEvent) => void;
  onUserStoppedTyping?: (groupId: string, userId: string) => void;
}

export function useSignalR(options: UseSignalROptions) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const joinedGroupsRef = useRef<Set<string>>(new Set());

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`/hubs/chat?access_token=${options.accessToken}`)
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    if (options.onMessageReceived) {
      connection.on("MessageReceived", options.onMessageReceived);
    }
    if (options.onMemberAdded) {
      connection.on("MemberAdded", options.onMemberAdded);
    }
    if (options.onMemberRemoved) {
      connection.on("MemberRemoved", options.onMemberRemoved);
    }
    if (options.onUserTyping) {
      connection.on("UserTyping", options.onUserTyping);
    }
    if (options.onUserStoppedTyping) {
      connection.on("UserStoppedTyping", options.onUserStoppedTyping);
    }

    // Re-join groups on reconnect
    connection.onreconnected(() => {
      joinedGroupsRef.current.forEach((groupId) => {
        connection.invoke("JoinGroup", groupId).catch(console.error);
      });
    });

    // Start connection
    connection.start().catch(console.error);
    connectionRef.current = connection;

    // Cleanup
    return () => {
      connection.stop();
    };
  }, [options.accessToken]);

  const joinGroup = useCallback(async (groupId: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("JoinGroup", groupId);
      joinedGroupsRef.current.add(groupId);
    }
  }, []);

  const leaveGroup = useCallback(async (groupId: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("LeaveGroup", groupId);
      joinedGroupsRef.current.delete(groupId);
    }
  }, []);

  const startTyping = useCallback(async (groupId: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("StartTyping", groupId);
    }
  }, []);

  const stopTyping = useCallback(async (groupId: string) => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      await connectionRef.current.invoke("StopTyping", groupId);
    }
  }, []);

  return { joinGroup, leaveGroup, startTyping, stopTyping };
}
```

### 8. TypeScript Types

```typescript
// Event types for frontend
interface MessageResponse {
  id: string;
  groupId: string;
  senderId: string | null;
  senderUserName: string | null;
  senderDisplayName: string | null;
  senderType: "User" | "AI";
  content: string;
  attachmentUrl: string | null;
  attachmentType: string | null;
  attachmentName: string | null;
  createdAt: string;
}

interface AiSettingsChangedEvent {
  groupId: string;
  aiMonitoringEnabled: boolean;
  aiProviderId: string;
  aiProviderName: string;
}

interface MemberAddedEvent {
  groupId: string;
  member: GroupMemberResponse;
}

interface MemberRemovedEvent {
  groupId: string;
  userId: string;
}

interface MemberRoleChangedEvent {
  groupId: string;
  userId: string;
  newRole: string;
}

interface UserTypingEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
}

interface GroupMemberResponse {
  userId: string;
  userName: string;
  displayName: string;
  role: string;
  joinedAt: string;
}
```

---

## Testing

### Unit Tests

SignalR broadcasts are tested by mocking `IChatHubService`:

```csharp
// Test that MessageService broadcasts when sending a message
[Fact]
public async Task SendMessageAsync_BroadcastsMessageToGroup()
{
    // Arrange
    // ... setup mocks

    // Act
    await MessageService.SendMessageAsync(groupId, request, userId);

    // Assert
    ChatHubServiceMock.Verify(
        x => x.BroadcastMessageAsync(
            groupId,
            It.Is<MessageResponse>(m => m.Content == "Hello"),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### Test Files

| File                                | Tests | What's Tested                     |
| ----------------------------------- | ----- | --------------------------------- |
| `SendMessageBroadcastTests.cs`      | 2     | Message broadcasting              |
| `UpdateAiSettingsBroadcastTests.cs` | 2     | AI settings broadcasting          |
| `MemberBroadcastTests.cs`           | 5     | Member add/remove/role broadcasts |

### Manual Testing

1. Start the API: `dotnet run --project src/AiGroupChat.API`
2. Use a SignalR test client or browser console
3. Connect with valid JWT token
4. Join a group and send messages via REST API
5. Verify events are received via WebSocket

---

## Summary

| Component         | Location                 | Purpose                            |
| ----------------- | ------------------------ | ---------------------------------- |
| `ChatHub`         | API/Hubs                 | WebSocket endpoint, client methods |
| `ChatHubService`  | API/Services             | Broadcasts from services           |
| `IChatHubService` | Application/Interfaces   | Abstraction for broadcasting       |
| Event DTOs        | Application/DTOs/SignalR | Event payload shapes               |

The key architectural decision is keeping `IChatHubService` in the Application layer as an abstraction. This allows:

1. Services to broadcast events without SignalR dependency
2. Easy unit testing with mocked hub service
3. Potential future replacement of SignalR with another technology
