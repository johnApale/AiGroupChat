# AI Group Chat

A group chat application with integrated AI agent capabilities. Users can create chat groups, invite members, and collaborate with an AI assistant that monitors and participates in conversations when invoked.

## Features (MVP)

- **User Authentication** - Register, login, JWT-based sessions with refresh tokens
- **Group Management** - Create groups, add/remove members, admin roles
- **Real-time Messaging** - Send messages via REST, receive via WebSocket (SignalR)
- **AI Monitoring Toggle** - Admin-only toggle; AI only sees messages sent while enabled
- **AI Invocation** - Any member can @mention AI to ask questions
- **Multi-provider Support** - Architecture supports Gemini, Claude, OpenAI, Grok

## Tech Stack

| Component      | Technology                           |
| -------------- | ------------------------------------ |
| Main Backend   | ASP.NET Core 9                       |
| Real-time      | SignalR                              |
| Database       | PostgreSQL 16                        |
| ORM            | Entity Framework Core 9              |
| Authentication | ASP.NET Identity + JWT               |
| AI Service     | Python FastAPI (separate repository) |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- IDE: VS Code, Visual Studio, or Rider

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd AiGroupChat
```

### 2. Start the database

```bash
docker compose up -d
```

This starts PostgreSQL on port `5434`.

### 3. Apply database migrations

```bash
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API
```

### 4. Run the application

```bash
dotnet run --project src/AiGroupChat.API
```

The API will be available at `https://localhost:5001` (or check console output for the actual port).

## Project Structure

This project follows **Clean Architecture** principles:

```
AiGroupChat/
├── src/
│   ├── AiGroupChat.API/            # Web API, Controllers, SignalR Hubs
│   ├── AiGroupChat.Application/    # Business logic, Services, DTOs
│   ├── AiGroupChat.Domain/         # Entities, Enums, Interfaces
│   └── AiGroupChat.Infrastructure/ # EF Core, External services
├── tests/
│   ├── AiGroupChat.UnitTests/
│   └── AiGroupChat.IntegrationTests/
├── docker-compose.yml              # Development PostgreSQL
└── AiGroupChat.sln
```

### Layer Dependencies

```
API → Infrastructure → Application → Domain
```

- **Domain** - Core entities and business logic. No external dependencies.
- **Application** - Use cases, DTOs, service interfaces. Depends on Domain.
- **Infrastructure** - Database, external APIs, implementations. Depends on Application.
- **API** - HTTP endpoints, SignalR hubs, middleware. Depends on Infrastructure.

## Development

### Useful Commands

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Add a new migration
dotnet ef migrations add <MigrationName> --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Update database
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Start PostgreSQL
docker compose up -d

# Stop PostgreSQL
docker compose down

# View PostgreSQL logs
docker logs aigroupchat-db
```

### Database Connection

- **Host**: localhost
- **Port**: 5434
- **Database**: aigroupchat_dev
- **User**: aigroupchat
- **Password**: devpassword123

## Architecture Overview

```
┌─────────────────┐         ┌─────────────────┐
│     Clients     │         │  Python AI      │
│  (Web/Mobile)   │         │  Service        │
└────────┬────────┘         └────────▲────────┘
         │                           │
         │ REST + WebSocket          │ HTTP
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

## License

[Add your license here]
