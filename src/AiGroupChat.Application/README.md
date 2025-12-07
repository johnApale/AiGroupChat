# AiGroupChat.Application

The Application layer contains business logic, DTOs, and service interfaces. This layer orchestrates the flow of data between the API and Infrastructure layers.

## Responsibilities

- Define DTOs for API requests/responses
- Define service interfaces (abstractions)
- Implement business logic in services
- Define custom exceptions

## Dependencies

- **AiGroupChat.Domain** - Core entities

## Folder Structure

```
AiGroupChat.Application/
├── DTOs/
│   └── Auth/
│       ├── RegisterRequest.cs
│       ├── LoginRequest.cs
│       ├── ConfirmEmailRequest.cs
│       ├── ResendConfirmationRequest.cs
│       ├── ForgotPasswordRequest.cs
│       ├── ResetPasswordRequest.cs
│       ├── RefreshTokenRequest.cs
│       ├── LogoutRequest.cs
│       ├── AuthResponse.cs
│       └── MessageResponse.cs
├── Exceptions/
│   ├── AuthenticationException.cs
│   ├── ValidationException.cs
│   └── NotFoundException.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── ITokenService.cs
│   ├── IUserRepository.cs
│   └── IEmailService.cs
├── Models/
│   └── EmailResult.cs
├── Services/
│   └── AuthService.cs
└── DependencyInjection.cs
```

## DTOs

DTOs (Data Transfer Objects) define the shape of data for API requests and responses.

| DTO                         | Purpose                                      |
| --------------------------- | -------------------------------------------- |
| `RegisterRequest`           | User registration input                      |
| `LoginRequest`              | User login input                             |
| `ConfirmEmailRequest`       | Email confirmation input                     |
| `ResendConfirmationRequest` | Resend confirmation email input              |
| `ForgotPasswordRequest`     | Password reset request input                 |
| `ResetPasswordRequest`      | Password reset input                         |
| `RefreshTokenRequest`       | Token refresh input                          |
| `LogoutRequest`             | Logout input                                 |
| `AuthResponse`              | Authentication response with tokens and user |
| `MessageResponse`           | Simple message response                      |
| `UserResponse`              | User profile data                            |
| `CreateGroupRequest`        | Group creation input                         |
| `UpdateGroupRequest`        | Group update input                           |
| `GroupResponse`             | Group details with members                   |
| `GroupMemberResponse`       | Group member profile data                    |

## Interfaces

Interfaces define contracts that are implemented in other layers.

| Interface             | Implemented In | Purpose                           |
| --------------------- | -------------- | --------------------------------- |
| `IAuthService`        | Application    | Authentication business logic     |
| `ITokenService`       | Infrastructure | JWT and refresh token handling    |
| `IUserRepository`     | Infrastructure | User data access (wraps Identity) |
| `IEmailService`       | Email          | Email sending                     |
| `IUserService`        | Application    | User profile retrieval            |
| `IGroupRepository`    | Infrastructure | Group data access                 |
| `IGroupService`       | Application    | Group CRUD and authorization      |
| `IGroupMemberService` | Application    | Group member management           |

## Exceptions

Custom exceptions for consistent error handling.

| Exception                 | HTTP Status | Usage                                   |
| ------------------------- | ----------- | --------------------------------------- |
| `AuthenticationException` | 401         | Invalid credentials, unconfirmed email  |
| `ValidationException`     | 400         | Invalid input, business rule violations |
| `NotFoundException`       | 404         | Resource not found                      |
| `AuthorizationException`  | 403         | User lacks permission for action        |

## Services

| Service              | Purpose                                                                  |
| -------------------- | ------------------------------------------------------------------------ |
| `AuthService`        | Handles registration, login, password reset, token refresh               |
| `UserService`        | Handles user lookup by ID and current user retrieval                     |
| `GroupService`       | Handles group creation, retrieval, update, delete with authorization     |
| `GroupMemberService` | Handles member add/remove, role changes, leave group, transfer ownership |

## Usage

Register services in DI:

```csharp
services.AddApplication();
```

## Design Decisions

1. **Interfaces in Application layer** - Following Dependency Inversion Principle, interfaces are defined here and implemented in outer layers.

2. **Custom exceptions** - Enable centralized error handling with consistent API responses.

3. **IEmailService here, not in Email project** - Keeps Application layer independent of email implementation details.
