# API Reference

Complete REST API documentation for the AI Group Chat application.

**Base URL**: `http://localhost:5126/api` (development)  
**Interactive Docs**: `http://localhost:5126/scalar/v1`

---

## Quick Links

| Section                             | Description                             |
| ----------------------------------- | --------------------------------------- |
| [Authentication](authentication.md) | Register, login, tokens, password reset |
| [Users](users.md)                   | User profile operations                 |
| [Groups](groups.md)                 | Group management                        |
| [Group Members](group-members.md)   | Member management, roles, ownership     |
| [Messages](messages.md)             | Send and retrieve messages              |
| [AI Providers](ai-providers.md)     | Available AI providers                  |
| [TypeScript Types](types.md)        | Complete type definitions               |

---

## Authentication

All endpoints except registration and login require a valid JWT token.

Include the token in the `Authorization` header:

```
Authorization: Bearer <access_token>
```

### Token Lifecycle

| Token         | Expiration | Storage Recommendation            |
| ------------- | ---------- | --------------------------------- |
| Access Token  | 15 minutes | Memory (React state)              |
| Refresh Token | 7 days     | HttpOnly cookie or secure storage |

### Token Refresh Strategy

```typescript
// Check if token expires soon (e.g., within 1 minute)
function isTokenExpiringSoon(expiresAt: string): boolean {
  const expiry = new Date(expiresAt).getTime();
  const now = Date.now();
  const oneMinute = 60 * 1000;
  return expiry - now < oneMinute;
}

// Refresh before making API calls
async function apiCall(endpoint: string, options: RequestInit) {
  if (isTokenExpiringSoon(tokenExpiresAt)) {
    const newTokens = await refreshTokens();
    updateStoredTokens(newTokens);
  }

  return fetch(endpoint, {
    ...options,
    headers: {
      ...options.headers,
      Authorization: `Bearer ${accessToken}`,
    },
  });
}
```

---

## Error Handling

All errors follow a consistent format:

```json
{
  "error": "ErrorType",
  "message": "Human-readable description",
  "details": ["Array of specific issues (for validation errors)"]
}
```

### HTTP Status Codes

| Status | Meaning                                 |
| ------ | --------------------------------------- |
| 200    | Success                                 |
| 201    | Created                                 |
| 204    | No Content (successful delete)          |
| 400    | Bad Request (validation error)          |
| 401    | Unauthorized (missing or invalid token) |
| 403    | Forbidden (insufficient permissions)    |
| 404    | Not Found                               |
| 500    | Internal Server Error                   |

### Common Error Types

| Error              | Status | Description               |
| ------------------ | ------ | ------------------------- |
| ValidationError    | 400    | Request validation failed |
| InvalidCredentials | 401    | Wrong email or password   |
| InvalidToken       | 401    | Token expired or invalid  |
| EmailNotConfirmed  | 401    | Email not yet verified    |
| Forbidden          | 403    | Insufficient permissions  |
| NotFound           | 404    | Resource not found        |

### Error Handling Example

```typescript
async function apiCall<T>(endpoint: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
      ...options?.headers,
    },
  });

  if (!response.ok) {
    const error = await response.json();

    switch (response.status) {
      case 401:
        // Token expired or invalid - redirect to login
        if (error.error === "InvalidToken") {
          logout();
          redirect("/login");
        }
        break;
      case 403:
        // Permission denied
        showToast("You do not have permission to perform this action");
        break;
      case 404:
        // Resource not found
        showToast(error.message);
        break;
      case 400:
        // Validation error - show details
        if (error.details) {
          error.details.forEach((detail: string) => showToast(detail));
        } else {
          showToast(error.message);
        }
        break;
      default:
        showToast("An unexpected error occurred");
    }

    throw new ApiError(error);
  }

  return response.json();
}
```

---

## Request/Response Conventions

### Dates

All dates are in ISO 8601 format (UTC):

```json
{
  "createdAt": "2025-01-15T10:30:00Z"
}
```

### IDs

- User IDs: String (GUID format)
- All other IDs: String (GUID format)

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440000"
}
```

### Pagination

Paginated endpoints return:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalCount": 127,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

Query parameters:

- `page` - Page number (1-based, default: 1)
- `pageSize` - Items per page (default: 50, max: 100)

---

## Example: Complete Auth Flow

```typescript
const API_BASE = "http://localhost:5126/api";

// 1. Register
const registerResponse = await fetch(`${API_BASE}/auth/register`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "john@example.com",
    userName: "johndoe",
    displayName: "John Doe",
    password: "SecurePass123!",
  }),
});
// User receives confirmation email

// 2. Confirm email (user clicks link, frontend extracts token)
const confirmResponse = await fetch(`${API_BASE}/auth/confirm-email`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "john@example.com",
    token: tokenFromUrl,
  }),
});
const { accessToken, refreshToken, expiresAt, user } =
  await confirmResponse.json();

// 3. Use access token for API calls
const groupsResponse = await fetch(`${API_BASE}/groups`, {
  headers: { Authorization: `Bearer ${accessToken}` },
});
const groups = await groupsResponse.json();

// 4. Refresh token before expiry
const refreshResponse = await fetch(`${API_BASE}/auth/refresh`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ refreshToken }),
});
const newTokens = await refreshResponse.json();

// 5. Logout
await fetch(`${API_BASE}/auth/logout`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ refreshToken: newTokens.refreshToken }),
});
```
