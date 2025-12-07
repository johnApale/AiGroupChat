# AiGroupChat.API

The API layer is the entry point for the application. It handles HTTP requests, authentication middleware, and routes requests to the appropriate services.

## Responsibilities

- HTTP endpoints (Controllers)
- Request/response handling
- Authentication and authorization middleware
- Global exception handling
- API documentation (Scalar)

## Dependencies

- **AiGroupChat.Infrastructure** (and transitively Application, Domain, Email)
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication
- **Scalar.AspNetCore** - API documentation

## Folder Structure

```
AiGroupChat.API/
├── Controllers/
│   └── AuthController.cs
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

| Controller               | Route                     | Purpose                     |
| ------------------------ | ------------------------- | --------------------------- |
| `AuthController`         | `/api/auth`               | Authentication endpoints    |
| `UsersController`        | `/api/users`              | User profile endpoints      |
| `GroupsController`       | `/api/groups`             | Group management endpoints  |
| `GroupMembersController` | `/api/groups/:id/members` | Member management endpoints |
| `GroupOwnerController`   | `/api/groups/:id/owner`   | Ownership transfer endpoint |

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

| Method | Endpoint          | Description                    | Auth Required |
| ------ | ----------------- | ------------------------------ | ------------- |
| POST   | `/api/groups`     | Create group (become admin)    | Yes           |
| GET    | `/api/groups`     | List my groups with members    | Yes           |
| GET    | `/api/groups/:id` | Get group details with members | Yes (member)  |
| PUT    | `/api/groups/:id` | Update group name              | Yes (admin)   |
| DELETE | `/api/groups/:id` | Delete group                   | Yes (admin)   |

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

**Request:**

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

**Response (201):**

```json
{
  "message": "Registration successful. Please check your email to confirm your account."
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
