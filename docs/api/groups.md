# Groups API

Endpoints for group management.

**Base URL**: `/api/groups`  
**Authentication**: Required for all endpoints

---

## Endpoints

| Method | Endpoint                     | Description        | Required Role |
| ------ | ---------------------------- | ------------------ | ------------- |
| POST   | [/](#post-create)            | Create group       | Any           |
| GET    | [/](#get-list)               | List my groups     | Any           |
| GET    | [/{id}](#get-by-id)          | Get group details  | Member        |
| PUT    | [/{id}](#put-update)         | Update group       | Admin         |
| DELETE | [/{id}](#delete)             | Delete group       | Owner         |
| PUT    | [/{id}/ai](#put-ai-settings) | Update AI settings | Admin         |

---

## POST / {#post-create}

Create a new group. The creator becomes the Owner.

### Request

```
POST /api/groups
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "name": "Project Alpha Team"
}
```

| Field | Type   | Required | Constraints      |
| ----- | ------ | -------- | ---------------- |
| name  | string | Yes      | 1-200 characters |

### Response

**201 Created**

```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "name": "Project Alpha Team",
  "createdById": "550e8400-e29b-41d4-a716-446655440000",
  "aiMonitoringEnabled": false,
  "aiProviderId": "770e8400-e29b-41d4-a716-446655440000",
  "aiProvider": {
    "id": "770e8400-e29b-41d4-a716-446655440000",
    "name": "gemini",
    "displayName": "Google Gemini",
    "defaultModel": "gemini-1.5-pro",
    "defaultTemperature": 0.7,
    "maxTokensLimit": 128000
  },
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z",
  "members": [
    {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "userName": "johndoe",
      "displayName": "John Doe",
      "role": "Owner",
      "joinedAt": "2025-01-15T10:30:00Z"
    }
  ]
}
```

### Errors

| Status | Error           | Description                  |
| ------ | --------------- | ---------------------------- |
| 400    | ValidationError | Name is required or too long |
| 401    | Unauthorized    | Not authenticated            |

### Example

```typescript
const response = await fetch("/api/groups", {
  method: "POST",
  headers: {
    Authorization: `Bearer ${accessToken}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({ name: "Project Alpha Team" }),
});

if (response.status === 201) {
  const group = await response.json();
  // Navigate to new group
  redirect(`/groups/${group.id}`);
}
```

---

## GET / {#get-list}

List all groups the current user is a member of.

### Request

```
GET /api/groups
Authorization: Bearer <access_token>
```

### Response

**200 OK**

```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "name": "Project Alpha Team",
    "createdById": "550e8400-e29b-41d4-a716-446655440000",
    "aiMonitoringEnabled": true,
    "aiProviderId": "770e8400-e29b-41d4-a716-446655440000",
    "aiProvider": {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "name": "gemini",
      "displayName": "Google Gemini",
      "defaultModel": "gemini-1.5-pro",
      "defaultTemperature": 0.7,
      "maxTokensLimit": 128000
    },
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-01-15T14:20:00Z",
    "members": [
      {
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "userName": "johndoe",
        "displayName": "John Doe",
        "role": "Owner",
        "joinedAt": "2025-01-15T10:30:00Z"
      },
      {
        "userId": "880e8400-e29b-41d4-a716-446655440000",
        "userName": "janedoe",
        "displayName": "Jane Doe",
        "role": "Member",
        "joinedAt": "2025-01-15T11:00:00Z"
      }
    ]
  }
]
```

### Example

```typescript
const response = await fetch("/api/groups", {
  headers: { Authorization: `Bearer ${accessToken}` },
});

const groups = await response.json();
setGroups(groups);
```

---

## GET /{id} {#get-by-id}

Get detailed information about a specific group.

### Request

```
GET /api/groups/{id}
Authorization: Bearer <access_token>
```

| Parameter | Type   | Description     |
| --------- | ------ | --------------- |
| id        | string | Group ID (GUID) |

### Response

**200 OK** - Same structure as create response

### Errors

| Status | Error        | Description                |
| ------ | ------------ | -------------------------- |
| 401    | Unauthorized | Not authenticated          |
| 403    | Forbidden    | Not a member of this group |
| 404    | NotFound     | Group not found            |

### Example

```typescript
async function getGroup(groupId: string): Promise<GroupResponse> {
  const response = await fetch(`/api/groups/${groupId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (response.status === 403) {
    throw new Error("You are not a member of this group");
  }
  if (response.status === 404) {
    throw new Error("Group not found");
  }

  return response.json();
}
```

---

## PUT /{id} {#put-update}

Update group details. Requires Admin or Owner role.

### Request

```
PUT /api/groups/{id}
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "name": "Project Alpha Team - Phase 2"
}
```

| Field | Type   | Required | Constraints      |
| ----- | ------ | -------- | ---------------- |
| name  | string | Yes      | 1-200 characters |

### Response

**200 OK** - Updated group object

### Errors

| Status | Error           | Description                |
| ------ | --------------- | -------------------------- |
| 400    | ValidationError | Invalid name               |
| 401    | Unauthorized    | Not authenticated          |
| 403    | Forbidden       | Not an admin of this group |
| 404    | NotFound        | Group not found            |

### Example

```typescript
const response = await fetch(`/api/groups/${groupId}`, {
  method: "PUT",
  headers: {
    Authorization: `Bearer ${accessToken}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({ name: "Project Alpha Team - Phase 2" }),
});

if (response.ok) {
  const updatedGroup = await response.json();
  setGroup(updatedGroup);
}
```

---

## DELETE /{id} {#delete}

Delete a group permanently. **This cannot be undone.**

Requires Owner role.

### Request

```
DELETE /api/groups/{id}
Authorization: Bearer <access_token>
```

### Response

**204 No Content**

### Errors

| Status | Error        | Description                 |
| ------ | ------------ | --------------------------- |
| 401    | Unauthorized | Not authenticated           |
| 403    | Forbidden    | Not the owner of this group |
| 404    | NotFound     | Group not found             |

### Example

```typescript
async function deleteGroup(groupId: string): Promise<void> {
  const confirmed = await showConfirmDialog(
    "Are you sure? This will delete all messages and cannot be undone."
  );

  if (!confirmed) return;

  const response = await fetch(`/api/groups/${groupId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (response.status === 204) {
    showToast("Group deleted");
    redirect("/groups");
  } else if (response.status === 403) {
    showToast("Only the owner can delete this group");
  }
}
```

---

## PUT /{id}/ai {#put-ai-settings}

Configure AI monitoring and provider for the group.

Requires Admin or Owner role.

### Request

```
PUT /api/groups/{id}/ai
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "aiMonitoringEnabled": true,
  "aiProviderId": "770e8400-e29b-41d4-a716-446655440000"
}
```

| Field               | Type    | Required | Description                          |
| ------------------- | ------- | -------- | ------------------------------------ |
| aiMonitoringEnabled | boolean | No       | Enable/disable AI monitoring         |
| aiProviderId        | string  | No       | Provider ID from `/api/ai-providers` |

Both fields are optional - only include fields you want to change.

### Response

**200 OK** - Updated group object

### AI Monitoring Behavior

| Monitoring   | Effect                                         |
| ------------ | ---------------------------------------------- |
| **Enabled**  | New messages are visible to the AI for context |
| **Disabled** | New messages are hidden from AI                |

**Important:** Only messages sent while monitoring is ON are included in AI context. Turning monitoring on doesn't retroactively expose old messages.

### Errors

| Status | Error           | Description                 |
| ------ | --------------- | --------------------------- |
| 400    | ValidationError | Invalid provider ID         |
| 401    | Unauthorized    | Not authenticated           |
| 403    | Forbidden       | Not an admin of this group  |
| 404    | NotFound        | Group or provider not found |

### Example

```typescript
// Enable AI monitoring with Gemini
const response = await fetch(`/api/groups/${groupId}/ai`, {
  method: "PUT",
  headers: {
    Authorization: `Bearer ${accessToken}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({
    aiMonitoringEnabled: true,
    aiProviderId: geminiProviderId,
  }),
});

if (response.ok) {
  const updatedGroup = await response.json();
  setGroup(updatedGroup);
  showToast("AI monitoring enabled");
}
```

### SignalR Event

When AI settings change, all group members receive an `AiSettingsChanged` event:

```typescript
connection.on("AiSettingsChanged", (event) => {
  // Update group state
  setGroup((prev) => ({
    ...prev,
    aiMonitoringEnabled: event.aiMonitoringEnabled,
    aiProviderId: event.aiProviderId,
  }));

  // Show notification
  showToast(
    `${event.changedByName} ${
      event.aiMonitoringEnabled ? "enabled" : "disabled"
    } AI monitoring`
  );
});
```

---

## Group Response Object

All group endpoints return this structure:

```typescript
interface GroupResponse {
  id: string; // Unique identifier
  name: string; // Group name
  createdById: string; // Owner's user ID
  aiMonitoringEnabled: boolean; // Is AI monitoring on?
  aiProviderId: string; // Current AI provider
  aiProvider: AiProviderResponse; // Provider details
  createdAt: string; // Creation timestamp
  updatedAt: string; // Last update timestamp
  members: GroupMemberResponse[]; // List of members
}

interface GroupMemberResponse {
  userId: string;
  userName: string;
  displayName: string;
  role: "Owner" | "Admin" | "Member";
  joinedAt: string;
}

interface AiProviderResponse {
  id: string;
  name: string;
  displayName: string;
  defaultModel: string;
  defaultTemperature: number;
  maxTokensLimit: number;
}
```
