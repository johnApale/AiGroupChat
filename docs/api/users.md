# Users API

Endpoints for user profile operations.

**Base URL**: `/api/users`  
**Authentication**: Required for all endpoints

---

## Endpoints

| Method | Endpoint         | Description      |
| ------ | ---------------- | ---------------- |
| GET    | [/me](#get-me)   | Get current user |
| GET    | [/{id}](#get-id) | Get user by ID   |

---

## GET /me

Get the current authenticated user's profile.

### Request

```
GET /api/users/me
Authorization: Bearer <access_token>
```

### Response

**200 OK**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "userName": "johndoe",
  "displayName": "John Doe",
  "createdAt": "2025-01-10T08:00:00Z"
}
```

| Field       | Type   | Description                |
| ----------- | ------ | -------------------------- |
| id          | string | User's unique identifier   |
| email       | string | User's email address       |
| userName    | string | User's username            |
| displayName | string | User's display name        |
| createdAt   | string | Account creation timestamp |

### Errors

| Status | Error        | Description              |
| ------ | ------------ | ------------------------ |
| 401    | Unauthorized | Missing or invalid token |

### Example

```typescript
const response = await fetch("/api/users/me", {
  headers: { Authorization: `Bearer ${accessToken}` },
});

if (response.ok) {
  const user = await response.json();
  setCurrentUser(user);
}
```

### Use Cases

- Fetching user details on app load
- Refreshing user data after profile updates
- Verifying authentication state

---

## GET /{id}

Get any user's public profile by their ID.

### Request

```
GET /api/users/{id}
Authorization: Bearer <access_token>
```

| Parameter | Type   | Description    |
| --------- | ------ | -------------- |
| id        | string | User ID (GUID) |

### Response

**200 OK**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "userName": "johndoe",
  "displayName": "John Doe",
  "createdAt": "2025-01-10T08:00:00Z"
}
```

### Errors

| Status | Error        | Description              |
| ------ | ------------ | ------------------------ |
| 401    | Unauthorized | Missing or invalid token |
| 404    | NotFound     | User not found           |

### Example

```typescript
async function getUser(userId: string): Promise<UserResponse> {
  const response = await fetch(`/api/users/${userId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (!response.ok) {
    if (response.status === 404) {
      throw new Error("User not found");
    }
    throw new Error("Failed to fetch user");
  }

  return response.json();
}
```

### Use Cases

- Displaying member profiles in a group
- Showing user details in member lists
- Looking up users before adding to groups
