# Group Members API

Endpoints for managing group membership, roles, and ownership.

**Base URL**: `/api/groups/{groupId}/members`  
**Authentication**: Required for all endpoints

---

## Role Hierarchy

| Role       | Permissions                                                         |
| ---------- | ------------------------------------------------------------------- |
| **Owner**  | Full control - delete group, transfer ownership, manage all members |
| **Admin**  | Manage members (add, remove Members), update AI settings            |
| **Member** | View group, send messages                                           |

### Permission Matrix

| Action                | Owner | Admin | Member |
| --------------------- | ----- | ----- | ------ |
| View group & messages | ✓     | ✓     | ✓      |
| Send messages         | ✓     | ✓     | ✓      |
| Add members           | ✓     | ✓     | ✗      |
| Remove Members        | ✓     | ✓     | ✗      |
| Remove Admins         | ✓     | ✗     | ✗      |
| Change roles          | ✓     | ✗     | ✗      |
| Update AI settings    | ✓     | ✓     | ✗      |
| Update group name     | ✓     | ✓     | ✗      |
| Delete group          | ✓     | ✗     | ✗      |
| Transfer ownership    | ✓     | ✗     | ✗      |
| Leave group           | ✗\*   | ✓     | ✓      |

\*Owner must transfer ownership before leaving

---

## Endpoints

| Method | Endpoint                    | Description        | Required Role |
| ------ | --------------------------- | ------------------ | ------------- |
| POST   | [/](#post-add)              | Add member         | Admin         |
| GET    | [/](#get-list)              | List members       | Member        |
| PUT    | [/{userId}](#put-role)      | Update role        | Owner         |
| DELETE | [/{userId}](#delete-remove) | Remove member      | Admin\*       |
| DELETE | [/me](#delete-leave)        | Leave group        | Member        |
| PUT    | [/owner](#put-transfer)     | Transfer ownership | Owner         |

\*Admins can only remove Members, not other Admins or Owner

---

## POST / {#post-add}

Add a user to the group as a Member.

Requires Admin or Owner role.

### Request

```
POST /api/groups/{groupId}/members
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "userId": "880e8400-e29b-41d4-a716-446655440000"
}
```

| Field  | Type   | Required | Description    |
| ------ | ------ | -------- | -------------- |
| userId | string | Yes      | User ID to add |

### Response

**201 Created**

```json
{
  "userId": "880e8400-e29b-41d4-a716-446655440000",
  "userName": "janedoe",
  "displayName": "Jane Doe",
  "role": "Member",
  "joinedAt": "2025-01-15T11:00:00Z"
}
```

### Errors

| Status | Error           | Description                |
| ------ | --------------- | -------------------------- |
| 400    | ValidationError | User is already a member   |
| 401    | Unauthorized    | Not authenticated          |
| 403    | Forbidden       | Not an admin of this group |
| 404    | NotFound        | Group or user not found    |

### Example

```typescript
async function addMember(
  groupId: string,
  userId: string
): Promise<GroupMemberResponse> {
  const response = await fetch(`/api/groups/${groupId}/members`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ userId }),
  });

  if (response.status === 400) {
    const error = await response.json();
    throw new Error(error.message); // "User is already a member"
  }

  return response.json();
}
```

### SignalR Events

When a member is added:

1. **Group channel** - `MemberJoined` event to active viewers
2. **Personal channel** - `AddedToGroup` event to the new member

```typescript
// Active group viewers see this
connection.on("MemberJoined", (event) => {
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

// The new member sees this
connection.on("AddedToGroup", (event) => {
  showToast(`You were added to ${event.groupName}`);
  refetchGroups();
});
```

---

## GET / {#get-list}

List all members of a group.

Requires membership in the group.

### Request

```
GET /api/groups/{groupId}/members
Authorization: Bearer <access_token>
```

### Response

**200 OK**

```json
[
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
    "role": "Admin",
    "joinedAt": "2025-01-15T11:00:00Z"
  },
  {
    "userId": "990e8400-e29b-41d4-a716-446655440000",
    "userName": "bobsmith",
    "displayName": "Bob Smith",
    "role": "Member",
    "joinedAt": "2025-01-15T12:00:00Z"
  }
]
```

### Errors

| Status | Error        | Description                |
| ------ | ------------ | -------------------------- |
| 401    | Unauthorized | Not authenticated          |
| 403    | Forbidden    | Not a member of this group |
| 404    | NotFound     | Group not found            |

### Example

```typescript
const response = await fetch(`/api/groups/${groupId}/members`, {
  headers: { Authorization: `Bearer ${accessToken}` },
});

const members = await response.json();
setMembers(members);
```

---

## PUT /{userId} {#put-role}

Change a member's role.

Requires Owner role.

### Request

```
PUT /api/groups/{groupId}/members/{userId}
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "role": "Admin"
}
```

| Field | Type   | Required | Values              |
| ----- | ------ | -------- | ------------------- |
| role  | string | Yes      | `Admin` or `Member` |

### Response

**200 OK**

```json
{
  "userId": "880e8400-e29b-41d4-a716-446655440000",
  "userName": "janedoe",
  "displayName": "Jane Doe",
  "role": "Admin",
  "joinedAt": "2025-01-15T11:00:00Z"
}
```

### Errors

| Status | Error           | Description                 |
| ------ | --------------- | --------------------------- |
| 400    | ValidationError | Invalid role value          |
| 400    | ValidationError | Cannot change owner's role  |
| 401    | Unauthorized    | Not authenticated           |
| 403    | Forbidden       | Not the owner of this group |
| 404    | NotFound        | Group or member not found   |

### Example

```typescript
async function updateRole(
  groupId: string,
  userId: string,
  role: "Admin" | "Member"
): Promise<void> {
  const response = await fetch(`/api/groups/${groupId}/members/${userId}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ role }),
  });

  if (response.status === 400) {
    const error = await response.json();
    showToast(error.message);
    return;
  }

  if (response.ok) {
    const updatedMember = await response.json();
    setMembers((prev) =>
      prev.map((m) => (m.userId === userId ? updatedMember : m))
    );
  }
}
```

### SignalR Events

1. **Group channel** - `MemberRoleChanged` to active viewers
2. **Personal channel** - `RoleChanged` to the affected member

```typescript
// The affected member sees this
connection.on("RoleChanged", (event) => {
  showToast(`Your role in ${event.groupName} changed to ${event.newRole}`);
});
```

---

## DELETE /{userId} {#delete-remove}

Remove a member from the group.

### Permissions

- **Owner**: Can remove anyone except themselves
- **Admin**: Can only remove Members (not other Admins or Owner)

### Request

```
DELETE /api/groups/{groupId}/members/{userId}
Authorization: Bearer <access_token>
```

### Response

**204 No Content**

### Errors

| Status | Error           | Description                                 |
| ------ | --------------- | ------------------------------------------- |
| 400    | ValidationError | Cannot remove yourself (use leave endpoint) |
| 401    | Unauthorized    | Not authenticated                           |
| 403    | Forbidden       | Cannot remove this member (higher role)     |
| 404    | NotFound        | Group or member not found                   |

### Example

```typescript
async function removeMember(groupId: string, userId: string): Promise<void> {
  const confirmed = await showConfirmDialog(
    "Remove this member from the group?"
  );
  if (!confirmed) return;

  const response = await fetch(`/api/groups/${groupId}/members/${userId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (response.status === 204) {
    setMembers((prev) => prev.filter((m) => m.userId !== userId));
    showToast("Member removed");
  } else if (response.status === 403) {
    showToast("You cannot remove this member");
  }
}
```

### SignalR Events

1. **Group channel** - `MemberLeft` to active viewers
2. **Personal channel** - `RemovedFromGroup` to the removed member

---

## DELETE /me {#delete-leave}

Leave the group voluntarily.

### Request

```
DELETE /api/groups/{groupId}/members/me
Authorization: Bearer <access_token>
```

### Response

**204 No Content**

### Errors

| Status | Error           | Description                                   |
| ------ | --------------- | --------------------------------------------- |
| 400    | ValidationError | Owner cannot leave (transfer ownership first) |
| 401    | Unauthorized    | Not authenticated                             |
| 404    | NotFound        | Group not found or not a member               |

### Example

```typescript
async function leaveGroup(groupId: string): Promise<void> {
  const confirmed = await showConfirmDialog("Leave this group?");
  if (!confirmed) return;

  const response = await fetch(`/api/groups/${groupId}/members/me`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (response.status === 204) {
    redirect("/groups");
    showToast("You left the group");
  } else if (response.status === 400) {
    showToast("Transfer ownership before leaving");
  }
}
```

---

## PUT /owner {#put-transfer}

Transfer group ownership to another member.

Requires Owner role.

### Request

```
PUT /api/groups/{groupId}/owner
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "newOwnerUserId": "880e8400-e29b-41d4-a716-446655440000"
}
```

| Field          | Type   | Required | Description          |
| -------------- | ------ | -------- | -------------------- |
| newOwnerUserId | string | Yes      | User ID of new owner |

### What Happens

1. New owner gets the **Owner** role
2. Current owner is demoted to **Admin**
3. Both users receive SignalR notifications

### Response

**200 OK** - New owner's member object

```json
{
  "userId": "880e8400-e29b-41d4-a716-446655440000",
  "userName": "janedoe",
  "displayName": "Jane Doe",
  "role": "Owner",
  "joinedAt": "2025-01-15T11:00:00Z"
}
```

### Errors

| Status | Error           | Description                 |
| ------ | --------------- | --------------------------- |
| 400    | ValidationError | Cannot transfer to yourself |
| 400    | ValidationError | Target user is not a member |
| 401    | Unauthorized    | Not authenticated           |
| 403    | Forbidden       | Not the owner of this group |
| 404    | NotFound        | Group or user not found     |

### Example

```typescript
async function transferOwnership(
  groupId: string,
  newOwnerUserId: string
): Promise<void> {
  const confirmed = await showConfirmDialog(
    "Transfer ownership? You will become an Admin."
  );
  if (!confirmed) return;

  const response = await fetch(`/api/groups/${groupId}/owner`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ newOwnerUserId }),
  });

  if (response.ok) {
    // Refetch group to get updated roles
    const group = await fetchGroup(groupId);
    setGroup(group);
    showToast("Ownership transferred");
  }
}
```

### SignalR Events

Both users receive `RoleChanged` events on their personal channels:

```typescript
connection.on("RoleChanged", (event) => {
  if (event.newRole === "Owner") {
    showToast(`You are now the owner of ${event.groupName}`);
  } else {
    showToast(`You are now an ${event.newRole} in ${event.groupName}`);
  }
});
```

---

## Member Response Object

```typescript
interface GroupMemberResponse {
  userId: string; // User's unique ID
  userName: string; // Username
  displayName: string; // Display name
  role: "Owner" | "Admin" | "Member";
  joinedAt: string; // ISO 8601 timestamp
}
```
