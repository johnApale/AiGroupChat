# Authentication API

Endpoints for user registration, login, and session management.

**Base URL**: `/api/auth`

---

## Endpoints

| Method | Endpoint                                          | Description         | Auth |
| ------ | ------------------------------------------------- | ------------------- | ---- |
| POST   | [/register](#post-register)                       | Create account      | No   |
| POST   | [/login](#post-login)                             | Get tokens          | No   |
| POST   | [/confirm-email](#post-confirm-email)             | Verify email        | No   |
| POST   | [/resend-confirmation](#post-resend-confirmation) | Resend confirmation | No   |
| POST   | [/forgot-password](#post-forgot-password)         | Request reset       | No   |
| POST   | [/reset-password](#post-reset-password)           | Set new password    | No   |
| POST   | [/refresh](#post-refresh)                         | Refresh tokens      | No   |
| POST   | [/logout](#post-logout)                           | Revoke token        | No   |

---

## POST /register

Create a new user account.

**Two registration modes:**

1. **Regular registration** (no `inviteToken`): Sends a confirmation email. User must confirm before logging in.
2. **Invite-based registration** (with `inviteToken`): Email is auto-confirmed, user is added to the group, and auth tokens are returned immediately.

### Request

```json
{
  "email": "john.doe@example.com",
  "userName": "johndoe",
  "displayName": "John Doe",
  "password": "SecurePass123!",
  "inviteToken": "abc123-invite-token"
}
```

| Field       | Type   | Required | Description                                       |
| ----------- | ------ | -------- | ------------------------------------------------- |
| email       | string | Yes      | Valid email address                               |
| userName    | string | Yes      | 3-50 characters, unique                           |
| displayName | string | Yes      | Up to 100 characters                              |
| password    | string | Yes      | See requirements below                            |
| inviteToken | string | No       | Invitation token (if registering via invite link) |

### Password Requirements

- At least 6 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&\*)

### Response

**201 Created - Regular Registration**

```json
{
  "requiresEmailConfirmation": true,
  "message": "Registration successful. Please check your email to confirm your account.",
  "auth": null,
  "groupId": null
}
```

**201 Created - Invite-Based Registration**

```json
{
  "requiresEmailConfirmation": false,
  "message": "Registration successful. You have been added to the group.",
  "auth": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "expiresAt": "2025-01-15T10:45:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "john.doe@example.com",
      "userName": "johndoe",
      "displayName": "John Doe"
    }
  },
  "groupId": "660e8400-e29b-41d4-a716-446655440001"
}
```

| Field                     | Type    | Description                                        |
| ------------------------- | ------- | -------------------------------------------------- |
| requiresEmailConfirmation | boolean | `true` for regular, `false` for invite-based       |
| message                   | string  | Human-readable status message                      |
| auth                      | object  | Auth tokens (only for invite-based registration)   |
| groupId                   | string  | Group ID user was added to (only for invite-based) |

### Errors

| Status | Error           | Description                         |
| ------ | --------------- | ----------------------------------- |
| 400    | ValidationError | Invalid email format                |
| 400    | ValidationError | Password doesn't meet requirements  |
| 400    | ValidationError | Username already taken              |
| 400    | ValidationError | Email already registered            |
| 400    | ValidationError | Invalid invitation token            |
| 400    | ValidationError | Invitation has expired              |
| 400    | ValidationError | Invitation is no longer valid       |
| 400    | ValidationError | Email does not match the invitation |

### Example - Regular Registration

```typescript
const response = await fetch("/api/auth/register", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "john.doe@example.com",
    userName: "johndoe",
    displayName: "John Doe",
    password: "SecurePass123!",
  }),
});

if (response.status === 201) {
  const result = await response.json();
  if (result.requiresEmailConfirmation) {
    // Show "check your email" message
    showMessage("Please check your email to confirm your account.");
  }
}
```

### Example - Invite-Based Registration

```typescript
// User clicked invite link, frontend extracted token and showed registration form
const inviteToken = getInviteTokenFromUrl();

const response = await fetch("/api/auth/register", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "invited@example.com", // Must match the invitation email
    userName: "inviteduser",
    displayName: "Invited User",
    password: "SecurePass123!",
    inviteToken: inviteToken,
  }),
});

if (response.status === 201) {
  const result = await response.json();
  if (!result.requiresEmailConfirmation) {
    // User is already logged in and added to group
    setTokens(result.auth.accessToken, result.auth.refreshToken);
    redirect(`/groups/${result.groupId}`);
  }
}
```

---

## POST /login

Authenticate and receive tokens.

### Request

```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

| Field    | Type   | Required | Description      |
| -------- | ------ | -------- | ---------------- |
| email    | string | Yes      | Registered email |
| password | string | Yes      | Account password |

### Response

**200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "expiresAt": "2025-01-15T10:45:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "userName": "johndoe",
    "displayName": "John Doe"
  }
}
```

| Field        | Type   | Description                             |
| ------------ | ------ | --------------------------------------- |
| accessToken  | string | JWT for API authentication (15 min)     |
| refreshToken | string | Token to get new access token (7 days)  |
| expiresAt    | string | UTC timestamp when access token expires |
| user         | object | Authenticated user's profile            |

### Errors

| Status | Error              | Description             |
| ------ | ------------------ | ----------------------- |
| 401    | InvalidCredentials | Wrong email or password |
| 401    | EmailNotConfirmed  | Email not yet verified  |

### Example

```typescript
const response = await fetch("/api/auth/login", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email: "john.doe@example.com",
    password: "SecurePass123!",
  }),
});

if (response.ok) {
  const { accessToken, refreshToken, expiresAt, user } = await response.json();

  // Store tokens
  setAccessToken(accessToken);
  setRefreshToken(refreshToken);
  setTokenExpiry(expiresAt);

  // Update user state
  setUser(user);
} else if (response.status === 401) {
  const error = await response.json();
  if (error.error === "EmailNotConfirmed") {
    showMessage("Please confirm your email before logging in.");
  } else {
    showMessage("Invalid email or password.");
  }
}
```

---

## POST /confirm-email

Verify email address using token from confirmation email. On success, user is automatically logged in.

### Request

```json
{
  "email": "john.doe@example.com",
  "token": "Q2ZESjhBT0..."
}
```

| Field | Type   | Required | Description           |
| ----- | ------ | -------- | --------------------- |
| email | string | Yes      | Email to confirm      |
| token | string | Yes      | Token from email link |

### Response

**200 OK** - Same as login response (user is auto-logged in)

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "expiresAt": "2025-01-15T10:45:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "userName": "johndoe",
    "displayName": "John Doe"
  }
}
```

### Errors

| Status | Error        | Description              |
| ------ | ------------ | ------------------------ |
| 400    | InvalidToken | Token expired or invalid |

### Example

```typescript
// Extract token from URL: /confirm-email?token=xxx&email=user@example.com
const params = new URLSearchParams(window.location.search);
const token = params.get("token");
const email = params.get("email");

const response = await fetch("/api/auth/confirm-email", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ email, token }),
});

if (response.ok) {
  const { accessToken, refreshToken, user } = await response.json();
  // User is now logged in
  setTokens(accessToken, refreshToken);
  redirect("/dashboard");
} else {
  showMessage("Confirmation link is invalid or expired.");
}
```

---

## POST /resend-confirmation

Resend confirmation email.

### Request

```json
{
  "email": "john.doe@example.com"
}
```

### Response

**200 OK**

```json
{
  "message": "If an unconfirmed account exists with this email, a confirmation link has been sent."
}
```

> **Note:** Always returns 200 to prevent email enumeration attacks.

### Example

```typescript
await fetch("/api/auth/resend-confirmation", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ email: "john.doe@example.com" }),
});

showMessage("If your account exists, a new confirmation email has been sent.");
```

---

## POST /forgot-password

Request a password reset email.

### Request

```json
{
  "email": "john.doe@example.com"
}
```

### Response

**200 OK**

```json
{
  "message": "If an account exists with this email, a password reset link has been sent."
}
```

> **Note:** Always returns 200 to prevent email enumeration. Reset link expires in 1 hour.

### Example

```typescript
await fetch("/api/auth/forgot-password", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ email: "john.doe@example.com" }),
});

showMessage("If your account exists, a password reset email has been sent.");
```

---

## POST /reset-password

Reset password using token from email.

### Request

```json
{
  "email": "john.doe@example.com",
  "token": "Q2ZESjhBT0...",
  "newPassword": "NewSecurePass456!"
}
```

| Field       | Type   | Required | Description            |
| ----------- | ------ | -------- | ---------------------- |
| email       | string | Yes      | Account email          |
| token       | string | Yes      | Token from reset email |
| newPassword | string | Yes      | New password           |

### Response

**200 OK**

```json
{
  "message": "Password has been reset successfully. You can now log in with your new password."
}
```

### Errors

| Status | Error           | Description                        |
| ------ | --------------- | ---------------------------------- |
| 400    | InvalidToken    | Token expired or invalid           |
| 400    | ValidationError | Password doesn't meet requirements |

### Example

```typescript
// Extract from URL: /reset-password?token=xxx&email=user@example.com
const params = new URLSearchParams(window.location.search);
const token = params.get("token");
const email = params.get("email");

const response = await fetch("/api/auth/reset-password", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({
    email,
    token,
    newPassword: "NewSecurePass456!",
  }),
});

if (response.ok) {
  showMessage("Password reset successful. Please log in.");
  redirect("/login");
}
```

---

## POST /refresh

Exchange refresh token for new tokens.

### Request

```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### Response

**200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-refresh-token-uuid",
  "expiresAt": "2025-01-15T11:00:00Z",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john.doe@example.com",
    "userName": "johndoe",
    "displayName": "John Doe"
  }
}
```

> **Important:** The old refresh token is revoked (token rotation for security). Always store the new refresh token.

### Errors

| Status | Error        | Description                        |
| ------ | ------------ | ---------------------------------- |
| 401    | InvalidToken | Token expired, revoked, or invalid |

### Example

```typescript
async function refreshTokens(): Promise<AuthResponse> {
  const response = await fetch("/api/auth/refresh", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken: getStoredRefreshToken() }),
  });

  if (response.ok) {
    const tokens = await response.json();
    // Store new tokens (old refresh token is now invalid)
    setAccessToken(tokens.accessToken);
    setRefreshToken(tokens.refreshToken);
    setTokenExpiry(tokens.expiresAt);
    return tokens;
  } else {
    // Refresh token expired or revoked - user must log in again
    logout();
    redirect("/login");
    throw new Error("Session expired");
  }
}
```

---

## POST /logout

Revoke refresh token.

### Request

```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### Response

**200 OK**

```json
{
  "message": "Logged out successfully."
}
```

> **Note:** The access token remains valid until it expires (15 minutes). For immediate revocation, implement a token blacklist on the server.

### Example

```typescript
async function logout() {
  const refreshToken = getStoredRefreshToken();

  if (refreshToken) {
    await fetch("/api/auth/logout", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });
  }

  // Clear local storage
  clearTokens();
  clearUser();

  // Redirect to login
  redirect("/login");
}
```
