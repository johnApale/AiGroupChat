# AiGroupChat.Application

The Application layer contains business logic, DTOs, and service interfaces. This layer orchestrates the flow of data between the API and Infrastructure layers.

## Responsibilities

- Define DTOs for API requests/responses
- Define SignalR event DTOs for real-time communication
- Define service interfaces (abstractions)
- Implement business logic in services
- Define custom exceptions

## Dependencies

- **AiGroupChat.Domain** - Core entities

## Folder Structure

```
AiGroupChat.Application/
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   ├── ConfirmEmailRequest.cs
│   │   ├── ResendConfirmationRequest.cs
│   │   ├── ForgotPasswordRequest.cs
│   │   ├── ResetPasswordRequest.cs
│   │   ├── RefreshTokenRequest.cs
│   │   ├── LogoutRequest.cs
│   │   ├── AuthResponse.cs
│   │   └── MessageResponse.cs
│   ├── Groups/
│   │   ├── CreateGroupRequest.cs
│   │   ├── UpdateGroupRequest.cs
│   │   ├── AddMemberRequest.cs
│   │   ├── UpdateMemberRoleRequest.cs
│   │   ├── TransferOwnershipRequest.cs
│   │   ├── UpdateAiSettingsRequest.cs
│   │   ├── GroupResponse.cs
│   │   └── GroupMemberResponse.cs
│   ├── Messages/
│   │   ├── SendMessageRequest.cs
│   │   └── MessageResponse.cs
│   ├── Users/
│   │   └── UserResponse.cs
│   ├── AiProviders/
│   │   └── AiProviderResponse.cs
│   ├── Common/
│   │   └── PaginatedResponse.cs
│   └── SignalR/
│       ├── GroupChannel/
│       │   ├── MemberJoinedEvent.cs
│       │   ├── MemberLeftEvent.cs
│       │   ├── MemberRoleChangedEvent.cs
│       │   ├── AiSettingsChangedEvent.cs
│       │   ├── UserTypingEvent.cs
│       │   └── UserStoppedTypingEvent.cs
│       └── PersonalChannel/
│           ├── GroupActivityEvent.cs
│           ├── NewMessageNotificationEvent.cs
│           ├── AddedToGroupEvent.cs
│           ├── RemovedFromGroupEvent.cs
│           ├── RoleChangedEvent.cs
│           ├── UserOnlineEvent.cs
│           └── UserOfflineEvent.cs
├── Exceptions/
│   ├── AuthenticationException.cs
│   ├── AuthorizationException.cs
│   ├── ValidationException.cs
│   └── NotFoundException.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── ITokenService.cs
│   ├── IUserRepository.cs
│   ├── IUserService.cs
│   ├── IGroupRepository.cs
│   ├── IGroupService.cs
│   ├── IGroupMemberRepository.cs
│   ├── IGroupMemberService.cs
│   ├── IAiProviderRepository.cs
│   ├── IAiProviderService.cs
│   ├── IMessageRepository.cs
│   ├── IMessageService.cs
│   ├── IChatHubService.cs
│   ├── IConnectionTracker.cs
│   └── IEmailService.cs
├── Models/
│   └── EmailResult.cs
├── Services/
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── GroupService.cs
│   ├── GroupMemberService.cs
│   ├── AiProviderService.cs
│   └── MessageService.cs
└── DependencyInjection.cs
```

## DTOs

DTOs (Data Transfer Objects) define the shape of data for API requests and responses.

### REST API DTOs

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
| `UpdateAiSettingsRequest`   | AI settings update input (partial)           |
| `GroupResponse`             | Group details with members and AI provider   |
| `GroupMemberResponse`       | Group member profile data                    |
| `AiProviderResponse`        | AI provider details (id, name, model, etc.)  |
| `SendMessageRequest`        | Message creation input                       |
| `MessageResponse`           | Message details with sender info             |
| `PaginatedResponse<T>`      | Generic paginated response wrapper           |

### SignalR Group Channel Events

Events broadcast to users actively viewing a group chat.

| Event                    | Purpose                                   |
| ------------------------ | ----------------------------------------- |
| `MemberJoinedEvent`      | Member added to group                     |
| `MemberLeftEvent`        | Member removed or left group              |
| `MemberRoleChangedEvent` | Member's role changed (includes OldRole)  |
| `AiSettingsChangedEvent` | AI monitoring toggled or provider changed |
| `UserTypingEvent`        | User started typing                       |
| `UserStoppedTypingEvent` | User stopped typing                       |

### SignalR Personal Channel Events

Events sent to a user's personal channel for notifications.

| Event                         | Purpose                               |
| ----------------------------- | ------------------------------------- |
| `GroupActivityEvent`          | Activity in any group (for home page) |
| `NewMessageNotificationEvent` | New message notification (for badge)  |
| `AddedToGroupEvent`           | User was added to a group             |
| `RemovedFromGroupEvent`       | User was removed from a group         |
| `RoleChangedEvent`            | User's role changed in a group        |
| `UserOnlineEvent`             | Shared user came online               |
| `UserOfflineEvent`            | Shared user went offline              |

## Interfaces

Interfaces define contracts that are implemented in other layers.

| Interface                | Implemented In | Purpose                           |
| ------------------------ | -------------- | --------------------------------- |
| `IAuthService`           | Application    | Authentication business logic     |
| `ITokenService`          | Infrastructure | JWT and refresh token handling    |
| `IUserRepository`        | Infrastructure | User data access (wraps Identity) |
| `IUserService`           | Application    | User profile retrieval            |
| `IGroupRepository`       | Infrastructure | Group data access                 |
| `IGroupService`          | Application    | Group CRUD and authorization      |
| `IGroupMemberRepository` | Infrastructure | Group member queries              |
| `IGroupMemberService`    | Application    | Group member management           |
| `IAiProviderRepository`  | Infrastructure | AI provider data access           |
| `IAiProviderService`     | Application    | AI provider listing and retrieval |
| `IMessageRepository`     | Infrastructure | Message data access               |
| `IMessageService`        | Application    | Message sending and retrieval     |
| `IChatHubService`        | API            | SignalR real-time broadcasting    |
| `IConnectionTracker`     | API            | User connection tracking          |
| `IEmailService`          | Email          | Email sending                     |

## Exceptions

Custom exceptions for consistent error handling.

| Exception                 | HTTP Status | Usage                                   |
| ------------------------- | ----------- | --------------------------------------- |
| `AuthenticationException` | 401         | Invalid credentials, unconfirmed email  |
| `AuthorizationException`  | 403         | User lacks permission for action        |
| `ValidationException`     | 400         | Invalid input, business rule violations |
| `NotFoundException`       | 404         | Resource not found                      |

## Services

| Service              | Purpose                                                                  |
| -------------------- | ------------------------------------------------------------------------ |
| `AuthService`        | Handles registration, login, password reset, token refresh               |
| `UserService`        | Handles user lookup by ID and current user retrieval                     |
| `GroupService`       | Handles group creation, retrieval, update, delete with authorization     |
| `GroupMemberService` | Handles member add/remove, role changes, leave group, transfer ownership |
| `AiProviderService`  | Handles listing and retrieving AI providers                              |
| `MessageService`     | Handles sending messages and paginated retrieval                         |

## Usage

Register services in DI:

```csharp
services.AddApplication();
```

## Design Decisions

1. **Interfaces in Application layer** - Following Dependency Inversion Principle, interfaces are defined here and implemented in outer layers.

2. **Custom exceptions** - Enable centralized error handling with consistent API responses.

3. **IEmailService here, not in Email project** - Keeps Application layer independent of email implementation details.

4. **SignalR DTOs in Application layer** - Allows services to create events without depending on SignalR infrastructure. The `IChatHubService` interface abstracts the actual SignalR calls.

5. **Two-channel SignalR architecture** - Group channel for active chat viewers, personal channel for notifications. This scales better than subscribing users to all their groups.
