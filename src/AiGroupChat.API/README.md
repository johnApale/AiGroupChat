# AiGroupChat.API

The API layer is the entry point for the application. It handles HTTP requests, authentication middleware, SignalR real-time communication, and routes requests to the appropriate services.

## Responsibilities

- HTTP endpoints (Controllers)
- Request/response handling
- Authentication and authorization middleware
- Global exception handling
- SignalR hub for real-time communication
- Connection tracking for presence
- API documentation (Scalar)

## Dependencies

- **AiGroupChat.Infrastructure** (and transitively Application, Domain, Email)
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Scalar.AspNetCore** - API documentation

## Folder Structure

```
AiGroupChat.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── GroupsController.cs
│   ├── GroupMembersController.cs
│   ├── GroupOwnerController.cs
│   ├── AiProvidersController.cs
│   └── MessagesController.cs
├── Hubs/
│   └── ChatHub.cs
├── Services/
│   ├── ChatHubService.cs      # SignalR broadcasting implementation
│   └── ConnectionTracker.cs   # User connection tracking
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json  # (gitignored)
├── Program.cs
└── README.md
```

## Controllers

| Controller               | Route                      | Purpose                     |
| ------------------------ | -------------------------- | --------------------------- |
| `AuthController`         | `/api/auth`                | Authentication endpoints    |
| `UsersController`        | `/api/users`               | User profile endpoints      |
| `GroupsController`       | `/api/groups`              | Group management endpoints  |
| `GroupMembersController` | `/api/groups/:id/members`  | Member management endpoints |
| `GroupOwnerController`   | `/api/groups/:id/owner`    | Ownership transfer endpoint |
| `AiProvidersController`  | `/api/ai-providers`        | AI provider endpoints       |
| `MessagesController`     | `/api/groups/:id/messages` | Message endpoints           |

## SignalR Hub

| Hub       | Endpoint     | Purpose                               |
| --------- | ------------ | ------------------------------------- |
| `ChatHub` | `/hubs/chat` | Real-time messaging and notifications |

### Client → Server Methods

| Method                 | Parameters | Description                           |
| ---------------------- | ---------- | ------------------------------------- |
| `JoinGroup(groupId)`   | `Guid`     | Subscribe to group's real-time events |
| `LeaveGroup(groupId)`  | `Guid`     | Unsubscribe from group events         |
| `StartTyping(groupId)` | `Guid`     | Notify group that user started typing |
| `StopTyping(groupId)`  | `Guid`     | Notify group that user stopped typing |

### Server → Client Events (Group Channel)

Events sent to the SignalR group when users are actively viewing a chat.

| Event               | Payload                  | Description                    |
| ------------------- | ------------------------ | ------------------------------ |
| `MessageReceived`   | `MessageResponse`        | New message sent in group      |
| `AiSettingsChanged` | `AiSettingsChangedEvent` | Group AI settings were updated |
| `MemberJoined`      | `MemberJoinedEvent`      | New member joined the group    |
| `MemberLeft`        | `MemberLeftEvent`        | Member left or was removed     |
| `MemberRoleChanged` | `MemberRoleChangedEvent` | Member's role was changed      |
| `UserTyping`        | `UserTypingEvent`        | User started typing in group   |
| `UserStoppedTyping` | `UserStoppedTypingEvent` | User stopped typing            |
| `AiTyping`          | `AiTypingEvent`          | AI started generating response |
| `AiStoppedTyping`   | `AiStoppedTypingEvent`   | AI finished generating         |

### Server → Client Events (Personal Channel)

Events sent to a user's personal channel (`user_{userId}`) for notifications.

| Event                    | Payload                       | Description                    |
| ------------------------ | ----------------------------- | ------------------------------ |
| `GroupActivity`          | `GroupActivityEvent`          | Activity in any user's group   |
| `NewMessageNotification` | `NewMessageNotificationEvent` | New message notification       |
| `AddedToGroup`           | `AddedToGroupEvent`           | User was added to a group      |
| `RemovedFromGroup`       | `RemovedFromGroupEvent`       | User was removed from a group  |
| `RoleChanged`            | `RoleChangedEvent`            | User's role changed in a group |
| `UserOnline`             | `UserOnlineEvent`             | Shared user came online        |
| `UserOffline`            | `UserOfflineEvent`            | Shared user went offline       |

### SignalR Authentication

SignalR uses JWT authentication. Since WebSocket doesn't support headers, the token is passed via query string:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat?access_token=" + accessToken)
  .build();
```

### Connection Lifecycle

When a user connects to the hub:

1. User is added to their personal channel (`user_{userId}`)
2. `UserOnline` event is broadcast to users who share groups with them
3. Connection is tracked in `ConnectionTracker`

When a user disconnects:

1. `UserOffline` event is broadcast to users who share groups with them
2. Connection is removed from `ConnectionTracker`
3. User is removed from all SignalR groups

## Services

### ChatHubService

Implements `IChatHubService` from the Application layer. Handles all SignalR broadcasting:

- Group channel broadcasts (messages, member changes, AI settings)
- Personal channel notifications (activity, added/removed, role changes)
- Presence broadcasts (online/offline)

### ConnectionTracker

Implements `IConnectionTracker` from the Application layer. Thread-safe tracking of user connections:

- Maps connection IDs to user IDs
- Maps user IDs to connection IDs
- Supports multiple connections per user
- Used for presence detection and targeted messaging

## Authentication Endpoints

| Method | Endpoint                        | Description                                  |
| ------ | ------------------------------- | -------------------------------------------- |
| POST   | `/api/auth/register`            | Create new account, sends confirmation email |
| POST   | `/api/auth/login`               | Authenticate and receive tokens              |
| POST   | `/api/auth/confirm-email`       | Confirm email with token                     |
| POST   | `/api/auth/resend-confirmation` | Resend confirmation email                    |
| POST   | `/api/auth/forgot-password`     | Request password reset email                 |
| POST   | `/api/auth/reset-password`      | Reset password with token                    |
| POST   | `/api/auth/refresh`             | Refresh access token                         |
| POST   | `/api/auth/logout`              | Revoke refresh token                         |

## User Endpoints

| Method | Endpoint         | Description                    | Auth Required |
| ------ | ---------------- | ------------------------------ | ------------- |
| GET    | `/api/users/me`  | Get current authenticated user | Yes           |
| GET    | `/api/users/:id` | Get user by ID                 | Yes           |

## Group Endpoints

| Method | Endpoint             | Description                    | Auth Required |
| ------ | -------------------- | ------------------------------ | ------------- |
| POST   | `/api/groups`        | Create group (become admin)    | Yes           |
| GET    | `/api/groups`        | List my groups with members    | Yes           |
| GET    | `/api/groups/:id`    | Get group details with members | Yes (member)  |
| PUT    | `/api/groups/:id`    | Update group name              | Yes (admin)   |
| DELETE | `/api/groups/:id`    | Delete group                   | Yes (admin)   |
| PUT    | `/api/groups/:id/ai` | Update AI settings             | Yes (admin)   |

## Group Member Endpoints

| Method | Endpoint                          | Description        | Auth Required |
| ------ | --------------------------------- | ------------------ | ------------- |
| POST   | `/api/groups/:id/members`         | Add member         | Yes (admin)   |
| GET    | `/api/groups/:id/members`         | List members       | Yes (member)  |
| PUT    | `/api/groups/:id/members/:userId` | Update member role | Yes (owner)   |
| DELETE | `/api/groups/:id/members/:userId` | Remove member      | Yes (admin\*) |
| DELETE | `/api/groups/:id/members/me`      | Leave group        | Yes (member)  |
| PUT    | `/api/groups/:id/owner`           | Transfer ownership | Yes (owner)   |

\*Admin can only remove Members, Owner can remove anyone except themselves

## AI Provider Endpoints

| Method | Endpoint                | Description                | Auth Required |
| ------ | ----------------------- | -------------------------- | ------------- |
| GET    | `/api/ai-providers`     | List all enabled providers | Yes           |
| GET    | `/api/ai-providers/:id` | Get provider by ID         | Yes           |

## Message Endpoints

| Method | Endpoint                   | Description              | Auth Required |
| ------ | -------------------------- | ------------------------ | ------------- |
| POST   | `/api/groups/:id/messages` | Send message             | Yes (member)  |
| GET    | `/api/groups/:id/messages` | Get messages (paginated) | Yes (member)  |

## Middleware

### ExceptionHandlingMiddleware

Centralized exception handling that converts exceptions to consistent API responses.

| Exception                 | HTTP Status | Response                      |
| ------------------------- | ----------- | ----------------------------- |
| `AuthenticationException` | 401         | `{ error, message }`          |
| `ValidationException`     | 400         | `{ error, message, details }` |
| `NotFoundException`       | 404         | `{ error, message }`          |
| `AuthorizationException`  | 403         | `{ error, message }`          |
| Unhandled exceptions      | 500         | `{ error, message }`          |

## Configuration

### appsettings.json (committed)

Contains placeholder values and non-sensitive defaults:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Email": {
    "ApiKey": "",
    "FromEmail": "",
    "FromName": "AI Group Chat",
    "FrontendBaseUrl": "",
    "ConfirmEmailPath": "/confirm-email",
    "ResetPasswordPath": "/reset-password"
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "AiGroupChat",
    "Audience": "AiGroupChat",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "AiService": {
    "BaseUrl": "",
    "ApiKey": "",
    "TimeoutSeconds": 30,
    "MaxContextMessages": 100
  }
}
```

### appsettings.Development.json (gitignored)

Contains actual secrets for local development:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=aigroupchat_dev;Username=aigroupchat;Password=devpassword123"
  },
  "Email": {
    "ApiKey": "re_your_resend_api_key",
    "FromEmail": "noreply@yourdomain.com",
    "FrontendBaseUrl": "http://localhost:3000"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long"
  },
  "AiService": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": "dev-api-key-change-in-production"
  }
}
```

## Running the API

```bash
# Start the database
docker compose up -d

# Run the API
dotnet run --project src/AiGroupChat.API

# Access Scalar docs
open http://localhost:5126/scalar/v1
```

## Request/Response Examples

### Register

**Request (Regular):**

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "userName": "johndoe",
  "displayName": "John Doe",
  "password": "SecurePass123!"
}
```

**Response (201) - Regular Registration:**

```json
{
  "requiresEmailConfirmation": true,
  "message": "Registration successful. Please check your email to confirm your account.",
  "auth": null,
  "groupId": null
}
```

**Request (Invite-Based):**

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "invited@example.com",
  "userName": "inviteduser",
  "displayName": "Invited User",
  "password": "SecurePass123!",
  "inviteToken": "abc123-invite-token"
}
```

**Response (201) - Invite-Based Registration:**

```json
{
  "requiresEmailConfirmation": false,
  "message": "Registration successful. You have been added to the group.",
  "auth": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "expiresAt": "2025-01-15T10:45:00Z",
    "user": {
      "id": "user-uuid",
      "email": "invited@example.com",
      "userName": "inviteduser",
      "displayName": "Invited User"
    }
  },
  "groupId": "660e8400-e29b-41d4-a716-446655440001"
}
```

### Login

**Request:**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200):**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "expiresAt": "2025-01-15T10:45:00Z",
  "user": {
    "id": "user-uuid",
    "email": "user@example.com",
    "userName": "johndoe",
    "displayName": "John Doe"
  }
}
```

### Error Response

```json
{
  "error": "ValidationError",
  "message": "Passwords must have at least one uppercase ('A'-'Z').",
  "details": ["Passwords must have at least one uppercase ('A'-'Z')."]
}
```

## Design Decisions

1. **Scalar over Swagger** - Cleaner API documentation UI.

2. **Global exception middleware** - Centralized error handling keeps controllers clean and ensures consistent error responses.

3. **JWT in header** - Access tokens are passed via `Authorization: Bearer <token>` header.

4. **Refresh tokens in body** - Refresh tokens are passed in request body, not cookies, for better cross-platform support.

5. **Two-channel SignalR architecture** - Group channel for active chat viewers, personal channel for notifications. This allows efficient routing without subscribing users to all their groups at once.

6. **In-memory connection tracking** - Simple `ConcurrentDictionary` for MVP. Can be upgraded to Redis for horizontal scaling.
