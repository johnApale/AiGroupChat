# SignalR Real-time Guide

Frontend integration guide for real-time features in AI Group Chat.

---

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Connection Setup](#connection-setup)
- [Client Methods](#client-methods)
- [Server Events](#server-events)
- [React Integration](#react-integration)
- [TypeScript Types](#typescript-types)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

SignalR provides WebSocket-based real-time communication. Use it for:

- **Instant message delivery** - Messages appear immediately
- **Typing indicators** - See when others are typing
- **Live updates** - Member changes, AI responses, settings updates
- **Presence** - Online/offline status of users

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     YOUR FRONTEND APP                        │
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐   │
│  │  REST API    │    │   SignalR    │    │    State     │   │
│  │   Client     │    │  Connection  │    │  Management  │   │
│  └──────────────┘    └──────────────┘    └──────────────┘   │
│         │                   │                    │           │
│         │ Send messages     │ Receive events     │           │
│         │ CRUD operations   │ Real-time updates  │           │
└─────────┼───────────────────┼────────────────────┼───────────┘
          │                   │                    │
          ▼                   ▼                    │
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET CORE API                          │
│                                                              │
│     POST /api/messages ──────► SignalR Hub ──────► Clients   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Key Concept:** Send messages via REST API, receive them via SignalR.

---

## Quick Start

### 1. Install SignalR Client

```bash
npm install @microsoft/signalr
```

### 2. Connect to Hub

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5126/hubs/chat?access_token=" + accessToken)
  .withAutomaticReconnect()
  .build();

await connection.start();
```

### 3. Listen for Messages

```typescript
connection.on("MessageReceived", (message) => {
  console.log("New message:", message);
});
```

### 4. Join a Group

```typescript
await connection.invoke("JoinGroup", groupId);
```

---

## Connection Setup

### Basic Connection

```typescript
import * as signalR from "@microsoft/signalr";

function createConnection(accessToken: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/chat?access_token=${accessToken}`)
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
    .configureLogging(signalR.LogLevel.Information)
    .build();
}
```

### Connection Lifecycle

```typescript
const connection = createConnection(accessToken);

// Connection state changes
connection.onreconnecting((error) => {
  console.log("Reconnecting...", error);
  // Show reconnecting UI
});

connection.onreconnected((connectionId) => {
  console.log("Reconnected:", connectionId);
  // Re-join groups after reconnect
  rejoinGroups();
});

connection.onclose((error) => {
  console.log("Connection closed:", error);
  // Show disconnected UI, maybe trigger re-auth
});

// Start connection
try {
  await connection.start();
  console.log("Connected to SignalR");
} catch (error) {
  console.error("Connection failed:", error);
}
```

### Token Refresh Handling

When JWT expires, you need to reconnect with a new token:

```typescript
async function refreshConnectionToken(newToken: string) {
  // Stop current connection
  await connection.stop();

  // Create new connection with fresh token
  connection = createConnection(newToken);

  // Re-register all event handlers
  registerEventHandlers(connection);

  // Start and re-join groups
  await connection.start();
  await rejoinGroups();
}
```

---

## Client Methods

Methods you can call on the server:

### JoinGroup

Subscribe to a group's real-time events. **Required before receiving group events.**

```typescript
await connection.invoke("JoinGroup", groupId);
```

Call this when:

- User opens a group chat
- After reconnection

### LeaveGroup

Unsubscribe from a group's events.

```typescript
await connection.invoke("LeaveGroup", groupId);
```

Call this when:

- User navigates away from group chat
- Before disconnecting (optional, auto-cleaned on disconnect)

### StartTyping

Notify group members that you started typing.

```typescript
await connection.invoke("StartTyping", groupId);
```

### StopTyping

Notify group members that you stopped typing.

```typescript
await connection.invoke("StopTyping", groupId);
```

### Typing Indicator Implementation

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

function handleBlur(groupId: string) {
  // Stop typing when input loses focus
  if (typingTimeout) {
    clearTimeout(typingTimeout);
  }
  connection.invoke("StopTyping", groupId).catch(console.error);
}
```

---

## Server Events

### Two Channels

SignalR uses two channels for events:

| Channel              | Subscription              | Events                                             |
| -------------------- | ------------------------- | -------------------------------------------------- |
| **Group Channel**    | Call `JoinGroup(groupId)` | Messages, typing, member changes in active group   |
| **Personal Channel** | Automatic on connect      | Notifications, presence, added/removed from groups |

---

### Group Channel Events

Events for the group you're actively viewing.

#### MessageReceived

New message posted in the group.

```typescript
connection.on("MessageReceived", (message: MessageResponse) => {
  // Add message to chat
  setMessages((prev) => [...prev, message]);
});
```

**Payload:**

```typescript
interface MessageResponse {
  id: string;
  groupId: string;
  senderId: string | null;
  senderUserName: string | null;
  senderDisplayName: string | null;
  senderType: "user" | "ai";
  content: string;
  attachmentUrl: string | null;
  attachmentType: string | null;
  attachmentName: string | null;
  createdAt: string;
}
```

#### UserTyping

User started typing.

```typescript
connection.on("UserTyping", (event: UserTypingEvent) => {
  setTypingUsers((prev) => [...prev, event]);
});
```

**Payload:**

```typescript
interface UserTypingEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
}
```

#### UserStoppedTyping

User stopped typing.

```typescript
connection.on("UserStoppedTyping", (event: UserStoppedTypingEvent) => {
  setTypingUsers((prev) => prev.filter((u) => u.userId !== event.userId));
});
```

**Payload:**

```typescript
interface UserStoppedTypingEvent {
  groupId: string;
  userId: string;
}
```

#### AiTyping

AI started generating a response.

```typescript
connection.on("AiTyping", (event: AiTypingEvent) => {
  setAiTyping(true);
  setAiProvider(event.providerName);
});
```

**Payload:**

```typescript
interface AiTypingEvent {
  groupId: string;
  providerId: string;
  providerName: string;
}
```

#### AiStoppedTyping

AI finished generating (response will arrive via `MessageReceived`).

```typescript
connection.on("AiStoppedTyping", (event: AiStoppedTypingEvent) => {
  setAiTyping(false);
});
```

**Payload:**

```typescript
interface AiStoppedTypingEvent {
  groupId: string;
  providerId: string;
}
```

#### MemberJoined

New member added to the group.

```typescript
connection.on("MemberJoined", (event: MemberJoinedEvent) => {
  setMembers((prev) => [
    ...prev,
    {
      userId: event.userId,
      userName: event.userName,
      displayName: event.displayName,
      role: event.role,
      joinedAt: event.joinedAt,
    },
  ]);
});
```

**Payload:**

```typescript
interface MemberJoinedEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
  role: string;
  joinedAt: string;
}
```

#### MemberLeft

Member left or was removed.

```typescript
connection.on("MemberLeft", (event: MemberLeftEvent) => {
  setMembers((prev) => prev.filter((m) => m.userId !== event.userId));
});
```

**Payload:**

```typescript
interface MemberLeftEvent {
  groupId: string;
  userId: string;
  displayName: string;
  leftAt: string;
}
```

#### MemberRoleChanged

Member's role was updated.

```typescript
connection.on("MemberRoleChanged", (event: MemberRoleChangedEvent) => {
  setMembers((prev) =>
    prev.map((m) =>
      m.userId === event.userId ? { ...m, role: event.newRole } : m
    )
  );
});
```

**Payload:**

```typescript
interface MemberRoleChangedEvent {
  groupId: string;
  userId: string;
  displayName: string;
  oldRole: string;
  newRole: string;
}
```

#### AiSettingsChanged

AI monitoring or provider was updated.

```typescript
connection.on("AiSettingsChanged", (event: AiSettingsChangedEvent) => {
  setGroup((prev) => ({
    ...prev,
    aiMonitoringEnabled: event.aiMonitoringEnabled,
    aiProviderId: event.aiProviderId,
  }));
});
```

**Payload:**

```typescript
interface AiSettingsChangedEvent {
  groupId: string;
  aiMonitoringEnabled: boolean;
  aiProviderId: string | null;
  aiProviderName: string | null;
  changedByName: string;
  changedAt: string;
}
```

---

### Personal Channel Events

Events sent to you regardless of which group you're viewing.

#### AddedToGroup

You were added to a new group.

```typescript
connection.on("AddedToGroup", (event: AddedToGroupEvent) => {
  // Show notification
  showToast(`You were added to ${event.groupName}`);
  // Refresh group list
  refetchGroups();
});
```

**Payload:**

```typescript
interface AddedToGroupEvent {
  groupId: string;
  groupName: string;
  addedByName: string;
  role: string;
  addedAt: string;
}
```

#### RemovedFromGroup

You were removed from a group.

```typescript
connection.on("RemovedFromGroup", (event: RemovedFromGroupEvent) => {
  // Show notification
  showToast(`You were removed from ${event.groupName}`);
  // Refresh group list, navigate away if viewing that group
  refetchGroups();
  if (currentGroupId === event.groupId) {
    navigate("/");
  }
});
```

**Payload:**

```typescript
interface RemovedFromGroupEvent {
  groupId: string;
  groupName: string;
  removedAt: string;
}
```

#### RoleChanged

Your role in a group changed.

```typescript
connection.on("RoleChanged", (event: RoleChangedEvent) => {
  showToast(`Your role in ${event.groupName} changed to ${event.newRole}`);
});
```

**Payload:**

```typescript
interface RoleChangedEvent {
  groupId: string;
  groupName: string;
  oldRole: string;
  newRole: string;
  changedByName: string;
  changedAt: string;
}
```

#### NewMessageNotification

New message in any group you belong to (for notification badges).

```typescript
connection.on(
  "NewMessageNotification",
  (event: NewMessageNotificationEvent) => {
    // Update unread count
    incrementUnreadCount(event.groupId);
    // Show notification if not viewing that group
    if (currentGroupId !== event.groupId) {
      showNotification(event.groupName, event.preview);
    }
  }
);
```

**Payload:**

```typescript
interface NewMessageNotificationEvent {
  groupId: string;
  groupName: string;
  messageId: string;
  senderName: string;
  preview: string; // Truncated message content
  sentAt: string;
}
```

#### GroupActivity

Activity in any group (for reordering group list).

```typescript
connection.on("GroupActivity", (event: GroupActivityEvent) => {
  // Move group to top of list
  reorderGroups(event.groupId);
});
```

**Payload:**

```typescript
interface GroupActivityEvent {
  groupId: string;
  groupName: string;
  activityType: string;
  timestamp: string;
  preview: string | null;
  actorName: string | null;
}
```

#### UserOnline

A user who shares groups with you came online.

```typescript
connection.on("UserOnline", (event: UserOnlineEvent) => {
  setOnlineUsers((prev) => [...prev, event.userId]);
});
```

**Payload:**

```typescript
interface UserOnlineEvent {
  userId: string;
  displayName: string;
  onlineAt: string;
}
```

#### UserOffline

A user who shares groups with you went offline.

```typescript
connection.on("UserOffline", (event: UserOfflineEvent) => {
  setOnlineUsers((prev) => prev.filter((id) => id !== event.userId));
});
```

**Payload:**

```typescript
interface UserOfflineEvent {
  userId: string;
  displayName: string;
  offlineAt: string;
}
```

---

## React Integration

### useSignalR Hook

Complete React hook for SignalR integration:

```typescript
import { useEffect, useRef, useCallback, useState } from "react";
import * as signalR from "@microsoft/signalr";

interface UseSignalROptions {
  accessToken: string;
  baseUrl: string;
  onMessageReceived?: (message: MessageResponse) => void;
  onUserTyping?: (event: UserTypingEvent) => void;
  onUserStoppedTyping?: (event: UserStoppedTypingEvent) => void;
  onAiTyping?: (event: AiTypingEvent) => void;
  onAiStoppedTyping?: (event: AiStoppedTypingEvent) => void;
  onMemberJoined?: (event: MemberJoinedEvent) => void;
  onMemberLeft?: (event: MemberLeftEvent) => void;
  onMemberRoleChanged?: (event: MemberRoleChangedEvent) => void;
  onAiSettingsChanged?: (event: AiSettingsChangedEvent) => void;
  onAddedToGroup?: (event: AddedToGroupEvent) => void;
  onRemovedFromGroup?: (event: RemovedFromGroupEvent) => void;
  onRoleChanged?: (event: RoleChangedEvent) => void;
  onNewMessageNotification?: (event: NewMessageNotificationEvent) => void;
  onUserOnline?: (event: UserOnlineEvent) => void;
  onUserOffline?: (event: UserOfflineEvent) => void;
}

type ConnectionState =
  | "disconnected"
  | "connecting"
  | "connected"
  | "reconnecting";

export function useSignalR(options: UseSignalROptions) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const joinedGroupsRef = useRef<Set<string>>(new Set());
  const [connectionState, setConnectionState] =
    useState<ConnectionState>("disconnected");

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(
        `${options.baseUrl}/hubs/chat?access_token=${options.accessToken}`
      )
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    // Register event handlers
    const events = {
      MessageReceived: options.onMessageReceived,
      UserTyping: options.onUserTyping,
      UserStoppedTyping: options.onUserStoppedTyping,
      AiTyping: options.onAiTyping,
      AiStoppedTyping: options.onAiStoppedTyping,
      MemberJoined: options.onMemberJoined,
      MemberLeft: options.onMemberLeft,
      MemberRoleChanged: options.onMemberRoleChanged,
      AiSettingsChanged: options.onAiSettingsChanged,
      AddedToGroup: options.onAddedToGroup,
      RemovedFromGroup: options.onRemovedFromGroup,
      RoleChanged: options.onRoleChanged,
      NewMessageNotification: options.onNewMessageNotification,
      UserOnline: options.onUserOnline,
      UserOffline: options.onUserOffline,
    };

    Object.entries(events).forEach(([event, handler]) => {
      if (handler) {
        connection.on(event, handler);
      }
    });

    // Connection state handlers
    connection.onreconnecting(() => setConnectionState("reconnecting"));
    connection.onreconnected(() => {
      setConnectionState("connected");
      // Re-join all groups after reconnect
      joinedGroupsRef.current.forEach((groupId) => {
        connection.invoke("JoinGroup", groupId).catch(console.error);
      });
    });
    connection.onclose(() => setConnectionState("disconnected"));

    // Start connection
    setConnectionState("connecting");
    connection
      .start()
      .then(() => setConnectionState("connected"))
      .catch((err) => {
        console.error("SignalR connection failed:", err);
        setConnectionState("disconnected");
      });

    connectionRef.current = connection;

    // Cleanup
    return () => {
      connection.stop();
    };
  }, [options.accessToken, options.baseUrl]);

  const joinGroup = useCallback(async (groupId: string) => {
    const connection = connectionRef.current;
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("JoinGroup", groupId);
      joinedGroupsRef.current.add(groupId);
    }
  }, []);

  const leaveGroup = useCallback(async (groupId: string) => {
    const connection = connectionRef.current;
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("LeaveGroup", groupId);
      joinedGroupsRef.current.delete(groupId);
    }
  }, []);

  const startTyping = useCallback(async (groupId: string) => {
    const connection = connectionRef.current;
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("StartTyping", groupId);
    }
  }, []);

  const stopTyping = useCallback(async (groupId: string) => {
    const connection = connectionRef.current;
    if (connection?.state === signalR.HubConnectionState.Connected) {
      await connection.invoke("StopTyping", groupId);
    }
  }, []);

  return {
    connectionState,
    joinGroup,
    leaveGroup,
    startTyping,
    stopTyping,
  };
}
```

### Usage in Chat Component

```tsx
import { useSignalR } from "./hooks/useSignalR";
import { useState, useEffect } from "react";

function ChatRoom({
  groupId,
  accessToken,
}: {
  groupId: string;
  accessToken: string;
}) {
  const [messages, setMessages] = useState<MessageResponse[]>([]);
  const [typingUsers, setTypingUsers] = useState<UserTypingEvent[]>([]);
  const [aiTyping, setAiTyping] = useState(false);

  const { connectionState, joinGroup, leaveGroup, startTyping, stopTyping } =
    useSignalR({
      accessToken,
      baseUrl: "http://localhost:5126",
      onMessageReceived: (message) => {
        setMessages((prev) => [...prev, message]);
      },
      onUserTyping: (event) => {
        setTypingUsers((prev) => {
          if (prev.find((u) => u.userId === event.userId)) return prev;
          return [...prev, event];
        });
      },
      onUserStoppedTyping: (event) => {
        setTypingUsers((prev) => prev.filter((u) => u.userId !== event.userId));
      },
      onAiTyping: () => setAiTyping(true),
      onAiStoppedTyping: () => setAiTyping(false),
    });

  // Join group when component mounts
  useEffect(() => {
    if (connectionState === "connected") {
      joinGroup(groupId);
    }
    return () => {
      if (connectionState === "connected") {
        leaveGroup(groupId);
      }
    };
  }, [groupId, connectionState, joinGroup, leaveGroup]);

  // Typing indicator with debounce
  const handleInputChange = useDebouncedTyping(
    groupId,
    startTyping,
    stopTyping
  );

  return (
    <div>
      {connectionState !== "connected" && (
        <div className="connection-banner">
          {connectionState === "connecting" && "Connecting..."}
          {connectionState === "reconnecting" && "Reconnecting..."}
          {connectionState === "disconnected" && "Disconnected"}
        </div>
      )}

      <div className="messages">
        {messages.map((msg) => (
          <Message key={msg.id} message={msg} />
        ))}
      </div>

      {typingUsers.length > 0 && (
        <div className="typing-indicator">
          {typingUsers.map((u) => u.displayName).join(", ")} typing...
        </div>
      )}

      {aiTyping && <div className="ai-typing-indicator">AI is thinking...</div>}

      <input
        type="text"
        onChange={handleInputChange}
        placeholder="Type a message..."
      />
    </div>
  );
}

// Debounced typing hook
function useDebouncedTyping(
  groupId: string,
  startTyping: (groupId: string) => Promise<void>,
  stopTyping: (groupId: string) => Promise<void>
) {
  const timeoutRef = useRef<NodeJS.Timeout>();

  return useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      startTyping(groupId);

      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }

      timeoutRef.current = setTimeout(() => {
        stopTyping(groupId);
      }, 2000);
    },
    [groupId, startTyping, stopTyping]
  );
}
```

---

## TypeScript Types

Complete type definitions for all SignalR events:

```typescript
// ============================================
// MESSAGE TYPES
// ============================================

interface MessageResponse {
  id: string;
  groupId: string;
  senderId: string | null;
  senderUserName: string | null;
  senderDisplayName: string | null;
  senderType: "user" | "ai";
  content: string;
  attachmentUrl: string | null;
  attachmentType: string | null;
  attachmentName: string | null;
  createdAt: string;
}

// ============================================
// GROUP CHANNEL EVENTS
// ============================================

interface UserTypingEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
}

interface UserStoppedTypingEvent {
  groupId: string;
  userId: string;
}

interface AiTypingEvent {
  groupId: string;
  providerId: string;
  providerName: string;
}

interface AiStoppedTypingEvent {
  groupId: string;
  providerId: string;
}

interface MemberJoinedEvent {
  groupId: string;
  userId: string;
  userName: string;
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
  aiProviderId: string | null;
  aiProviderName: string | null;
  changedByName: string;
  changedAt: string;
}

// ============================================
// PERSONAL CHANNEL EVENTS
// ============================================

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

interface NewMessageNotificationEvent {
  groupId: string;
  groupName: string;
  messageId: string;
  senderName: string;
  preview: string;
  sentAt: string;
}

interface GroupActivityEvent {
  groupId: string;
  groupName: string;
  activityType: string;
  timestamp: string;
  preview: string | null;
  actorName: string | null;
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
```

---

## Best Practices

### 1. Always Re-join Groups on Reconnect

```typescript
connection.onreconnected(() => {
  joinedGroups.forEach((groupId) => {
    connection.invoke("JoinGroup", groupId);
  });
});
```

### 2. Handle Connection States in UI

```tsx
{
  connectionState === "reconnecting" && (
    <Banner type="warning">Reconnecting...</Banner>
  );
}
{
  connectionState === "disconnected" && (
    <Banner type="error">Connection lost. Retrying...</Banner>
  );
}
```

### 3. Deduplicate Messages

SignalR guarantees at-least-once delivery. Handle duplicates:

```typescript
onMessageReceived: (message) => {
  setMessages((prev) => {
    if (prev.find((m) => m.id === message.id)) {
      return prev; // Already have this message
    }
    return [...prev, message];
  });
};
```

### 4. Clean Up Typing Indicators

Remove stale typing indicators if stop event is missed:

```typescript
useEffect(() => {
  const interval = setInterval(() => {
    setTypingUsers((prev) =>
      prev.filter((u) => Date.now() - u.startedAt < 5000)
    );
  }, 1000);
  return () => clearInterval(interval);
}, []);
```

### 5. Token Refresh Strategy

Reconnect with new token before expiry:

```typescript
useEffect(() => {
  const checkToken = () => {
    if (isTokenExpiringSoon(accessToken)) {
      refreshToken().then((newToken) => {
        // Reconnect with new token
      });
    }
  };
  const interval = setInterval(checkToken, 60000);
  return () => clearInterval(interval);
}, [accessToken]);
```

---

## Troubleshooting

### Connection Fails with 401

**Cause:** Invalid or expired JWT token.

**Fix:** Ensure token is valid and not expired before connecting.

```typescript
if (isTokenExpired(accessToken)) {
  const newTokens = await refreshToken();
  accessToken = newTokens.accessToken;
}
```

### Not Receiving Group Events

**Cause:** Didn't call `JoinGroup` or membership check failed.

**Fix:** Ensure you call `JoinGroup` after connection is established:

```typescript
connection.start().then(() => {
  connection.invoke("JoinGroup", groupId);
});
```

### Duplicate Messages

**Cause:** SignalR reconnection can cause duplicate delivery.

**Fix:** Deduplicate by message ID (see Best Practices).

### Events Stop After Tab Inactive

**Cause:** Some browsers throttle background tabs.

**Fix:** SignalR's `withAutomaticReconnect` handles this, but you may want to refresh data when tab becomes active:

```typescript
document.addEventListener("visibilitychange", () => {
  if (document.visibilityState === "visible") {
    refetchMessages();
  }
});
```

### HubException: "You are not a member of this group"

**Cause:** Trying to join a group you're not a member of.

**Fix:** Only call `JoinGroup` for groups the user belongs to.

---

## Summary

| Action             | Method                                                    |
| ------------------ | --------------------------------------------------------- |
| Connect            | `new HubConnectionBuilder().withUrl(...).build().start()` |
| Subscribe to group | `connection.invoke("JoinGroup", groupId)`                 |
| Unsubscribe        | `connection.invoke("LeaveGroup", groupId)`                |
| Start typing       | `connection.invoke("StartTyping", groupId)`               |
| Stop typing        | `connection.invoke("StopTyping", groupId)`                |
| Listen for events  | `connection.on("EventName", callback)`                    |

**Key Points:**

- Send messages via REST API, receive via SignalR
- Always re-join groups after reconnection
- Handle all connection states in UI
- Deduplicate messages by ID
- Clean up event handlers on unmount
