# AI Group Chat

A group chat application with integrated AI agent capabilities. Users can create chat groups, invite members, and collaborate with AI assistants that monitor and participate in conversations when invoked.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## Features

- **🔐 Authentication** - Register, login, JWT + refresh tokens, email verification, password reset
- **👥 Groups** - Create groups, manage members, role-based permissions (Owner/Admin/Member)
- **💬 Real-time Messaging** - Send via REST, receive via SignalR WebSocket
- **🤖 AI Integration** - Multiple providers (Gemini, Claude, OpenAI, Grok)
- **🎛️ AI Monitoring** - Admin-controlled toggle; AI only sees messages sent while enabled

## Quick Start

```bash
# 1. Clone and navigate
git clone <repository-url>
cd AiGroupChat

# 2. Start PostgreSQL
docker compose up -d

# 3. Configure secrets (copy and edit)
cp src/AiGroupChat.API/appsettings.json src/AiGroupChat.API/appsettings.Development.json
# Edit appsettings.Development.json with your settings (see Configuration below)

# 4. Apply migrations
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# 5. Run the API
dotnet run --project src/AiGroupChat.API
```

**API**: http://localhost:5126  
**API Docs**: http://localhost:5126/scalar/v1

## Documentation

| Document                                         | Description                             |
| ------------------------------------------------ | --------------------------------------- |
| [API Reference](docs/api/README.md)              | REST API overview, auth, and errors     |
| ↳ [Authentication](docs/api/authentication.md)   | Register, login, tokens, password reset |
| ↳ [Users](docs/api/users.md)                     | User profile endpoints                  |
| ↳ [Groups](docs/api/groups.md)                   | Group management                        |
| ↳ [Group Members](docs/api/group-members.md)     | Members, roles, ownership               |
| ↳ [Messages](docs/api/messages.md)               | Send and retrieve messages              |
| ↳ [AI Providers](docs/api/ai-providers.md)       | Available AI providers                  |
| ↳ [TypeScript Types](docs/api/types.md)          | Type definitions for frontend           |
| [SignalR Guide](docs/signalr-guide.md)           | Real-time integration for frontend      |
| [Spec Document](docs/spec/ai-group-chat-spec.md) | Full technical specification            |

## Tech Stack

| Component  | Technology                     |
| ---------- | ------------------------------ |
| Backend    | ASP.NET Core 9                 |
| Real-time  | SignalR                        |
| Database   | PostgreSQL 16                  |
| ORM        | Entity Framework Core 9        |
| Auth       | ASP.NET Identity + JWT         |
| Email      | Resend                         |
| API Docs   | Scalar (OpenAPI)               |
| AI Service | Python FastAPI (separate repo) |

## Configuration

Create `src/AiGroupChat.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=aigroupchat_dev;Username=aigroupchat;Password=devpassword123"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long-for-security"
  },
  "Email": {
    "ApiKey": "re_your_resend_api_key",
    "FromEmail": "noreply@yourdomain.com",
    "FrontendBaseUrl": "http://localhost:3000"
  },
  "AiService": {
    "BaseUrl": "http://localhost:8000",
    "ApiKey": "your-ai-service-api-key"
  }
}
```

### Environment Variables

For production, use environment variables:

| Variable                               | Description                    |
| -------------------------------------- | ------------------------------ |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string   |
| `Jwt__Secret`                          | JWT signing key (min 32 chars) |
| `Email__ApiKey`                        | Resend API key                 |
| `Email__FromEmail`                     | Sender email address           |
| `Email__FrontendBaseUrl`               | Frontend URL for email links   |
| `AiService__BaseUrl`                   | Python AI service URL          |
| `AiService__ApiKey`                    | AI service API key             |

## API Overview

### Authentication

| Method | Endpoint                    | Description          |
| ------ | --------------------------- | -------------------- |
| POST   | `/api/auth/register`        | Create account       |
| POST   | `/api/auth/login`           | Get tokens           |
| POST   | `/api/auth/refresh`         | Refresh access token |
| POST   | `/api/auth/logout`          | Revoke refresh token |
| POST   | `/api/auth/confirm-email`   | Verify email         |
| POST   | `/api/auth/forgot-password` | Request reset        |
| POST   | `/api/auth/reset-password`  | Set new password     |

### Groups

| Method | Endpoint             | Description           |
| ------ | -------------------- | --------------------- |
| POST   | `/api/groups`        | Create group          |
| GET    | `/api/groups`        | List my groups        |
| GET    | `/api/groups/:id`    | Get group details     |
| PUT    | `/api/groups/:id`    | Update group          |
| DELETE | `/api/groups/:id`    | Delete group          |
| PUT    | `/api/groups/:id/ai` | Configure AI settings |

### Members

| Method | Endpoint                          | Description        |
| ------ | --------------------------------- | ------------------ |
| POST   | `/api/groups/:id/members`         | Add member         |
| GET    | `/api/groups/:id/members`         | List members       |
| PUT    | `/api/groups/:id/members/:userId` | Change role        |
| DELETE | `/api/groups/:id/members/:userId` | Remove member      |
| DELETE | `/api/groups/:id/members/me`      | Leave group        |
| PUT    | `/api/groups/:id/owner`           | Transfer ownership |

### Messages

| Method | Endpoint                   | Description             |
| ------ | -------------------------- | ----------------------- |
| POST   | `/api/groups/:id/messages` | Send message            |
| GET    | `/api/groups/:id/messages` | Get history (paginated) |

### AI Providers

| Method | Endpoint                | Description    |
| ------ | ----------------------- | -------------- |
| GET    | `/api/ai-providers`     | List providers |
| GET    | `/api/ai-providers/:id` | Get provider   |

## Real-time (SignalR)

Connect to `/hubs/chat` with JWT token:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat?access_token=" + accessToken)
  .withAutomaticReconnect()
  .build();
```

### Key Events

| Event                | Description           |
| -------------------- | --------------------- |
| `MessageReceived`    | New message in group  |
| `AiTyping`           | AI started generating |
| `AiResponseReceived` | AI response ready     |
| `MemberJoined`       | Member added to group |
| `MemberLeft`         | Member left/removed   |

See [SignalR Guide](docs/signalr-guide.md) for complete integration details.

## Project Structure

```
AiGroupChat/
├── src/
│   ├── AiGroupChat.API/            # Controllers, Hubs, Middleware
│   ├── AiGroupChat.Application/    # Services, DTOs, Interfaces
│   ├── AiGroupChat.Domain/         # Entities, Enums
│   ├── AiGroupChat.Infrastructure/ # EF Core, Repositories
│   └── AiGroupChat.Email/          # Email service
├── tests/
│   ├── AiGroupChat.UnitTests/
│   └── AiGroupChat.IntegrationTests/
├── docs/
│   ├── api-reference.md
│   ├── signalr-guide.md
│   └── spec/
└── docker-compose.yml
```

### Architecture

```
┌─────────────────┐         ┌─────────────────┐
│     Clients     │         │  Python AI      │
│  (Web/Mobile)   │         │  Service        │
└────────┬────────┘         └────────▲────────┘
         │                           │
         │ REST + SignalR            │ HTTP
         ▼                           │
┌─────────────────────────────────────────────┐
│            ASP.NET Core API                 │
│  ┌─────────┐ ┌─────────┐ ┌──────────────┐  │
│  │  Auth   │ │ Groups  │ │   SignalR    │  │
│  │         │ │Messages │ │   (Realtime) │  │
│  └─────────┘ └─────────┘ └──────────────┘  │
└─────────────────────┬───────────────────────┘
                      │
                      ▼
              ┌───────────────┐
              │  PostgreSQL   │
              └───────────────┘
```

## Development

### Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run with watch
dotnet watch run --project src/AiGroupChat.API

# Add migration
dotnet ef migrations add <Name> \
  --project src/AiGroupChat.Infrastructure \
  --startup-project src/AiGroupChat.API

# Update database
dotnet ef database update \
  --project src/AiGroupChat.Infrastructure \
  --startup-project src/AiGroupChat.API
```

### Database

```
Host: localhost
Port: 5434
Database: aigroupchat_dev
User: aigroupchat
Password: devpassword123
```

```bash
# Start
docker compose up -d

# Stop
docker compose down

# Logs
docker logs aigroupchat-db
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
