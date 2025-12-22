# Group Invitations API

Endpoints for inviting users to groups via email.

**Base URL**: `/api/groups/{groupId}/invitations`  
**Authentication**: Required for all endpoints except accept

---

## Overview

The invitation system allows group admins to invite users by email address. The flow works as follows:

1. **Admin sends invitation** - Email is sent with a unique token link
2. **Recipient clicks link** - Frontend calls accept endpoint with token
3. **Two outcomes**:
   - **Existing user**: Added to group immediately, receives auth tokens
   - **New user**: Returns registration prompt with email pre-filled

### Invitation Lifecycle

| Status   | Description                          |
| -------- | ------------------------------------ |
| Pending  | Invitation sent, awaiting acceptance |
| Accepted | User accepted and joined the group   |
| Revoked  | Admin cancelled the invitation       |

### Configuration

| Setting                     | Default | Description                   |
| --------------------------- | ------- | ----------------------------- |
| `Invitation:ExpirationDays` | 7       | Days until invitation expires |

---

## Endpoints

| Method | Endpoint                                | Description       | Auth Required |
| ------ | --------------------------------------- | ----------------- | ------------- |
| POST   | [/](#post-invite)                       | Send invitations  | Yes (Admin)   |
| GET    | [/](#get-pending)                       | List pending      | Yes (Admin)   |
| DELETE | [/{invitationId}](#delete-revoke)       | Revoke invitation | Yes (Admin)   |
| POST   | [/api/invitations/accept](#post-accept) | Accept invitation | No            |

---

## POST / {#post-invite}

Send invitations to one or more email addresses.

Requires Admin or Owner role.

### Request

```
POST /api/groups/{groupId}/invitations
Authorization: Bearer <access_token>
Content-Type: application/json
```

```json
{
  "emails": ["user1@example.com", "user2@example.com"]
}
```

| Field  | Type     | Required | Description                  |
| ------ | -------- | -------- | ---------------------------- |
| emails | string[] | Yes      | List of email addresses (1+) |

### Response

**200 OK**

```json
{
  "sent": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "groupId": "660e8400-e29b-41d4-a716-446655440000",
      "email": "user1@example.com",
      "status": "Pending",
      "invitedByUserName": "John Doe",
      "createdAt": "2025-01-15T10:30:00Z",
      "expiresAt": "2025-01-22T10:30:00Z",
      "lastSentAt": "2025-01-15T10:30:00Z",
      "sendCount": 1
    }
  ],
  "failed": [
    {
      "email": "existing@example.com",
      "reason": "User is already a member of this group."
    }
  ]
}
```

### Behavior

- **New invitation**: Creates invitation, sends email
- **Resend**: Updates existing pending invitation, resets expiry, increments `sendCount`
- **Existing member**: Returns in `failed` array
- **Invalid email**: Returns in `failed` array

### Errors

| Status | Error        | Description                |
| ------ | ------------ | -------------------------- |
| 401    | Unauthorized | Not authenticated          |
| 403    | Forbidden    | Not an admin of this group |
| 404    | NotFound     | Group not found            |

### Example

```typescript
async function inviteMembers(
  groupId: string,
  emails: string[]
): Promise<InviteMembersResponse> {
  const response = await fetch(`/api/groups/${groupId}/invitations`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ emails }),
  });

  const result = await response.json();

  // Show results to user
  if (result.sent.length > 0) {
    showToast(`Sent ${result.sent.length} invitation(s)`);
  }
  if (result.failed.length > 0) {
    result.failed.forEach((f) => showToast(`${f.email}: ${f.reason}`));
  }

  return result;
}
```

---

## GET / {#get-pending}

List all pending invitations for a group.

Requires Admin or Owner role.

### Request

```
GET /api/groups/{groupId}/invitations
Authorization: Bearer <access_token>
```

### Response

**200 OK**

```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "groupId": "660e8400-e29b-41d4-a716-446655440000",
    "email": "pending@example.com",
    "status": "Pending",
    "invitedByUserName": "John Doe",
    "createdAt": "2025-01-15T10:30:00Z",
    "expiresAt": "2025-01-22T10:30:00Z",
    "lastSentAt": "2025-01-15T10:30:00Z",
    "sendCount": 1
  }
]
```

### Errors

| Status | Error        | Description                |
| ------ | ------------ | -------------------------- |
| 401    | Unauthorized | Not authenticated          |
| 403    | Forbidden    | Not an admin of this group |
| 404    | NotFound     | Group not found            |

### Example

```typescript
async function getPendingInvitations(
  groupId: string
): Promise<InvitationResponse[]> {
  const response = await fetch(`/api/groups/${groupId}/invitations`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  return response.json();
}
```

---

## DELETE /{invitationId} {#delete-revoke}

Revoke a pending invitation.

Requires Admin or Owner role.

### Request

```
DELETE /api/groups/{groupId}/invitations/{invitationId}
Authorization: Bearer <access_token>
```

### Response

**204 No Content**

### Errors

| Status | Error           | Description                   |
| ------ | --------------- | ----------------------------- |
| 400    | ValidationError | Invitation is not pending     |
| 401    | Unauthorized    | Not authenticated             |
| 403    | Forbidden       | Not an admin of this group    |
| 404    | NotFound        | Group or invitation not found |

### Example

```typescript
async function revokeInvitation(
  groupId: string,
  invitationId: string
): Promise<void> {
  const response = await fetch(
    `/api/groups/${groupId}/invitations/${invitationId}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );

  if (response.status === 204) {
    showToast("Invitation revoked");
    refetchInvitations();
  }
}
```

---

## POST /api/invitations/accept {#post-accept}

Accept an invitation using the token from the email link.

**No authentication required** - this endpoint is public.

### Request

```
POST /api/invitations/accept
Content-Type: application/json
```

```json
{
  "token": "abc123..."
}
```

| Field | Type   | Required | Description                 |
| ----- | ------ | -------- | --------------------------- |
| token | string | Yes      | Token from invitation email |

### Response - Existing User

**200 OK**

User is added to the group and receives auth tokens:

```json
{
  "requiresRegistration": false,
  "groupId": "660e8400-e29b-41d4-a716-446655440000",
  "auth": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4-e5f6-7890...",
    "expiresAt": "2025-01-15T10:45:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "userName": "johndoe",
      "displayName": "John Doe"
    }
  }
}
```

### Response - New User

**200 OK**

User needs to register first:

```json
{
  "requiresRegistration": true,
  "email": "newuser@example.com",
  "groupName": "Project Team",
  "auth": null,
  "groupId": null
}
```

### Frontend Flow

```typescript
async function handleInvitationAccept(token: string): Promise<void> {
  const response = await fetch("/api/invitations/accept", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token }),
  });

  if (!response.ok) {
    const error = await response.json();
    showToast(error.message);
    return;
  }

  const result = await response.json();

  if (result.requiresRegistration) {
    // Redirect to signup with email pre-filled and invite token
    redirect(
      `/signup?email=${encodeURIComponent(
        result.email
      )}&group=${encodeURIComponent(
        result.groupName
      )}&inviteToken=${encodeURIComponent(token)}`
    );
  } else {
    // User is authenticated and added to group
    setAuthTokens(result.auth);
    redirect(`/groups/${result.groupId}`);
    showToast("You've joined the group!");
  }
}
```

### Registration with Invite Token

When a new user needs to register, pass the `inviteToken` to the registration endpoint. This will:

- Auto-confirm their email (they proved ownership by clicking the invite link)
- Add them to the group immediately
- Return auth tokens so they're logged in

See [POST /api/auth/register](authentication.md#post-register) for details.

```typescript
async function handleInviteRegistration(
  email: string,
  inviteToken: string,
  formData: RegistrationForm
): Promise<void> {
  const response = await fetch("/api/auth/register", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      email: email, // Must match the invitation email
      userName: formData.userName,
      displayName: formData.displayName,
      password: formData.password,
      inviteToken: inviteToken,
    }),
  });

  if (response.status === 201) {
    const result = await response.json();
    if (!result.requiresEmailConfirmation) {
      // User is logged in and added to group
      setAuthTokens(result.auth);
      redirect(`/groups/${result.groupId}`);
      showToast("Welcome! You've joined the group.");
    }
  }
}
```

### Errors

| Status | Error           | Description                    |
| ------ | --------------- | ------------------------------ |
| 400    | ValidationError | Token expired or already used  |
| 400    | ValidationError | Already a member of this group |
| 404    | NotFound        | Invalid token                  |

---

## Response Objects

### InvitationResponse

```typescript
interface InvitationResponse {
  id: string; // Invitation ID
  groupId: string; // Group ID
  email: string; // Invitee email
  status: "Pending" | "Accepted" | "Revoked";
  invitedByUserName: string; // Who sent the invitation
  createdAt: string; // Original invitation time
  expiresAt: string; // Expiration time
  lastSentAt: string; // Last email sent time
  sendCount: number; // Times invitation was sent
}
```

### InviteMembersResponse

```typescript
interface InviteMembersResponse {
  sent: InvitationResponse[]; // Successfully sent
  failed: InvitationError[]; // Failed to send
}

interface InvitationError {
  email: string; // Email that failed
  reason: string; // Why it failed
}
```

### AcceptInvitationResponse

```typescript
interface AcceptInvitationResponse {
  requiresRegistration: boolean;
  email?: string; // For registration form
  groupName?: string; // For display
  groupId?: string; // If user was added
  auth?: AuthResponse; // If user was authenticated
}
```

---

## Email Template

The invitation email includes:

- Group name
- Inviter's display name
- Accept button with tokenized link
- Expiration notice

**Link format**: `{FrontendBaseUrl}/accept-invitation?token={token}`
