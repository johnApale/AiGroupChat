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

## Interfaces

Interfaces define contracts that are implemented in other layers.

| Interface         | Implemented In | Purpose                           |
| ----------------- | -------------- | --------------------------------- |
| `IAuthService`    | Application    | Authentication business logic     |
| `ITokenService`   | Infrastructure | JWT and refresh token handling    |
| `IUserRepository` | Infrastructure | User data access (wraps Identity) |
| `IEmailService`   | Email          | Email sending                     |

## Exceptions

Custom exceptions for consistent error handling.

| Exception                 | HTTP Status | Usage                                   |
| ------------------------- | ----------- | --------------------------------------- |
| `AuthenticationException` | 401         | Invalid credentials, unconfirmed email  |
| `ValidationException`     | 400         | Invalid input, business rule violations |
| `NotFoundException`       | 404         | Resource not found                      |

## Services

| Service       | Purpose                                                    |
| ------------- | ---------------------------------------------------------- |
| `AuthService` | Handles registration, login, password reset, token refresh |

## Usage

Register services in DI:

```csharp
services.AddApplication();
```

## Design Decisions

1. **Interfaces in Application layer** - Following Dependency Inversion Principle, interfaces are defined here and implemented in outer layers.

2. **Custom exceptions** - Enable centralized error handling with consistent API responses.

3. **IEmailService here, not in Email project** - Keeps Application layer independent of email implementation details.
