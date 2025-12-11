# Messages API

Endpoints for sending and retrieving messages.

**Base URL**: `/api/groups/{groupId}/messages`  
**Authentication**: Required for all endpoints

---

## Overview

Messages are the core of group communication. The API uses a **hybrid approach**:

- **Send** messages via REST API (this endpoint)
- **Receive** messages in real-time via SignalR

This ensures reliable message delivery while providing instant updates.

---

## Endpoints

| Method | Endpoint          | Description         | Required Role |
| ------ | ----------------- | ------------------- | ------------- |
| POST   | [/](#post-send)   | Send message        | Member        |
| GET    | [/](#get-history) | Get message history | Member        |

---

## POST / {#post-send}

Send a message to the group.

### Request

```
POST /api/groups/{groupId}/messages
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "content": "@gemini What do you think about this approach?"
}
```

| Field   | Type   | Required | Constraints         |
| ------- | ------ | -------- | ------------------- |
| content | string | Yes      | 1-10,000 characters |

### AI Invocation

To invoke the AI, mention it using `@` followed by the provider name:

| Mention   | Provider         |
| --------- | ---------------- |
| `@gemini` | Google Gemini    |
| `@claude` | Anthropic Claude |
| `@openai` | OpenAI GPT       |
| `@grok`   | xAI Grok         |

**Requirements for AI response:**

1. AI monitoring must be enabled for the group
2. The mentioned provider must be the configured provider
3. Message content must include the @mention

### Response

**201 Created**

```json
{
  "id": "990e8400-e29b-41d4-a716-446655440000",
  "groupId": "660e8400-e29b-41d4-a716-446655440000",
  "senderId": "550e8400-e29b-41d4-a716-446655440000",
  "senderUserName": "johndoe",
  "senderDisplayName": "John Doe",
  "senderType": "user",
  "content": "@gemini What do you think about this approach?",
  "attachmentUrl": null,
  "attachmentType": null,
  "attachmentName": null,
  "createdAt": "2025-01-15T12:00:00Z"
}
```

### Errors

| Status | Error           | Description                       |
| ------ | --------------- | --------------------------------- |
| 400    | ValidationError | Message is empty                  |
| 400    | ValidationError | Message exceeds 10,000 characters |
| 401    | Unauthorized    | Not authenticated                 |
| 403    | Forbidden       | Not a member of this group        |
| 404    | NotFound        | Group not found                   |

### Example

```typescript
async function sendMessage(
  groupId: string,
  content: string
): Promise<MessageResponse> {
  const response = await fetch(`/api/groups/${groupId}/messages`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ content }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  return response.json();
}

// Usage
const message = await sendMessage(groupId, "Hello everyone!");
```

### SignalR Events

When you send a message:

1. **REST response** - You get the message back immediately
2. **SignalR `MessageReceived`** - All group members (including you) receive the event

```typescript
// Don't add message twice - REST response is for confirmation,
// SignalR event updates the UI
connection.on("MessageReceived", (message) => {
  setMessages((prev) => {
    // Deduplicate by ID
    if (prev.find((m) => m.id === message.id)) return prev;
    return [...prev, message];
  });
});
```

### AI Response Flow

When you @mention the AI:

1. Your message is sent (REST response + `MessageReceived` event)
2. `AiTyping` event is broadcast
3. AI generates response
4. `AiStoppedTyping` event is broadcast
5. AI message arrives via `MessageReceived` event

```typescript
// Show typing indicator
connection.on("AiTyping", (event) => {
  setAiTyping(true);
  setAiProviderName(event.providerName);
});

// Hide typing indicator
connection.on("AiStoppedTyping", () => {
  setAiTyping(false);
});

// AI message arrives like any other message
connection.on("MessageReceived", (message) => {
  if (message.senderType === "ai") {
    // AI response received
  }
  setMessages((prev) => [...prev, message]);
});
```

---

## GET / {#get-history}

Get paginated message history for the group.

### Request

```
GET /api/groups/{groupId}/messages?page=1&pageSize=50
Authorization: Bearer <access_token>
```

| Parameter | Type | Default | Description                 |
| --------- | ---- | ------- | --------------------------- |
| page      | int  | 1       | Page number (1-based)       |
| pageSize  | int  | 50      | Messages per page (max 100) |

### Response

**200 OK**

```json
{
  "items": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440000",
      "groupId": "660e8400-e29b-41d4-a716-446655440000",
      "senderId": null,
      "senderUserName": null,
      "senderDisplayName": "Gemini",
      "senderType": "ai",
      "content": "That's a great approach because...",
      "attachmentUrl": null,
      "attachmentType": null,
      "attachmentName": null,
      "createdAt": "2025-01-15T12:00:05Z"
    },
    {
      "id": "990e8400-e29b-41d4-a716-446655440000",
      "groupId": "660e8400-e29b-41d4-a716-446655440000",
      "senderId": "550e8400-e29b-41d4-a716-446655440000",
      "senderUserName": "johndoe",
      "senderDisplayName": "John Doe",
      "senderType": "user",
      "content": "@gemini What do you think about this approach?",
      "attachmentUrl": null,
      "attachmentType": null,
      "attachmentName": null,
      "createdAt": "2025-01-15T12:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 50,
  "totalCount": 127,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

Messages are returned in **reverse chronological order** (newest first).

### Errors

| Status | Error        | Description                |
| ------ | ------------ | -------------------------- |
| 401    | Unauthorized | Not authenticated          |
| 403    | Forbidden    | Not a member of this group |
| 404    | NotFound     | Group not found            |

### Example

```typescript
interface PaginatedMessages {
  items: MessageResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

async function getMessages(
  groupId: string,
  page: number = 1,
  pageSize: number = 50
): Promise<PaginatedMessages> {
  const response = await fetch(
    `/api/groups/${groupId}/messages?page=${page}&pageSize=${pageSize}`,
    { headers: { Authorization: `Bearer ${accessToken}` } }
  );

  return response.json();
}

// Load initial messages
const { items, hasNextPage } = await getMessages(groupId);
setMessages(items.reverse()); // Reverse to show oldest first in UI

// Load more (infinite scroll)
async function loadMore() {
  if (!hasNextPage || isLoading) return;

  setIsLoading(true);
  const nextPage = currentPage + 1;
  const { items, hasNextPage: more } = await getMessages(groupId, nextPage);

  setMessages((prev) => [...items.reverse(), ...prev]);
  setCurrentPage(nextPage);
  setHasNextPage(more);
  setIsLoading(false);
}
```

### Pagination Strategy

For chat applications, you typically want to:

1. **Initial load**: Fetch page 1 (most recent messages)
2. **Scroll up**: Load older pages as user scrolls
3. **Real-time**: Append new messages from SignalR

```typescript
function ChatMessages({ groupId }) {
  const [messages, setMessages] = useState<MessageResponse[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const containerRef = useRef<HTMLDivElement>(null);

  // Initial load
  useEffect(() => {
    loadMessages(1);
  }, [groupId]);

  // Real-time messages
  useEffect(() => {
    connection.on("MessageReceived", (message) => {
      if (message.groupId === groupId) {
        setMessages((prev) => [...prev, message]);
      }
    });
    return () => connection.off("MessageReceived");
  }, [groupId]);

  // Infinite scroll - load older messages
  const handleScroll = () => {
    const container = containerRef.current;
    if (container && container.scrollTop === 0 && hasMore) {
      loadMessages(page + 1);
    }
  };

  async function loadMessages(pageNum: number) {
    const { items, hasNextPage } = await getMessages(groupId, pageNum);

    if (pageNum === 1) {
      setMessages(items.reverse());
    } else {
      // Prepend older messages
      setMessages((prev) => [...items.reverse(), ...prev]);
    }

    setPage(pageNum);
    setHasMore(hasNextPage);
  }

  return (
    <div ref={containerRef} onScroll={handleScroll}>
      {messages.map((msg) => (
        <Message key={msg.id} message={msg} />
      ))}
    </div>
  );
}
```

---

## Message Response Object

```typescript
interface MessageResponse {
  id: string; // Unique message ID
  groupId: string; // Group this message belongs to
  senderId: string | null; // User ID (null for AI)
  senderUserName: string | null; // Username (null for AI)
  senderDisplayName: string | null; // Display name or AI provider name
  senderType: "user" | "ai"; // Message source
  content: string; // Message text
  attachmentUrl: string | null; // Attachment URL (if any)
  attachmentType: string | null; // MIME type (e.g., "image/png")
  attachmentName: string | null; // Original filename
  createdAt: string; // ISO 8601 timestamp
}
```

### Identifying Message Types

```typescript
function isAiMessage(message: MessageResponse): boolean {
  return message.senderType === "ai";
}

function isMyMessage(message: MessageResponse, myUserId: string): boolean {
  return message.senderId === myUserId;
}

// Render differently based on type
function Message({ message, currentUserId }) {
  if (isAiMessage(message)) {
    return <AiMessageBubble message={message} />;
  }

  if (isMyMessage(message, currentUserId)) {
    return <MyMessageBubble message={message} />;
  }

  return <OtherMessageBubble message={message} />;
}
```

---

## AI Context

The AI only sees messages that were sent while **AI monitoring was enabled**. This is controlled by the `ai_visible` flag on each message (set automatically at creation time).

If a user asks the AI about something discussed before monitoring was enabled, the AI won't have that context.

```
Timeline:
├── Message 1: "Let's use React"        [AI monitoring OFF - AI can't see]
├── Message 2: "Good idea"              [AI monitoring OFF - AI can't see]
├── [Admin enables AI monitoring]
├── Message 3: "What about Vue?"        [AI monitoring ON - AI can see]
├── Message 4: "@gemini thoughts?"      [AI monitoring ON - AI can see]
└── AI Response: (only has context of messages 3-4)
```
