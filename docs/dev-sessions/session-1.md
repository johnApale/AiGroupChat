# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 1: Project Setup & Infrastructure

**Date:** December 6, 2025

### Completed

#### 1. Project Initialization

- Created .NET 9 solution with Clean Architecture structure
- Four projects established:
  - `AiGroupChat.Domain` - Core entities, no dependencies
  - `AiGroupChat.Application` - Business logic (empty, ready for services)
  - `AiGroupChat.Infrastructure` - EF Core, database access
  - `AiGroupChat.API` - Web API entry point
- Project references configured: API → Infrastructure → Application → Domain

#### 2. Domain Layer

- **Entities created:**

  - `User` (extends IdentityUser) - application user with DisplayName
  - `Group` - chat group with AI monitoring toggle
  - `GroupMember` - user membership with role (Admin/Member)
  - `Message` - chat message (user or AI) with attachment support
  - `AiProvider` - AI provider configuration (Gemini, Claude, etc.)
  - `AiResponseMetadata` - token usage, latency, cost tracking
  - `RefreshToken` - JWT refresh token storage

- **Enums created:**
  - `SenderType` - User, Ai
  - `GroupRole` - Member, Admin

#### 3. Infrastructure Layer

- **Database:** PostgreSQL 16 running in Docker (port 5434)
- **DbContext:** `ApplicationDbContext` with Identity integration
- **Entity Configurations:** All 7 entities configured with:
  - Snake_case table/column naming
  - Proper indexes and constraints
  - Foreign key relationships
  - Enum to string conversions
- **Dependency Injection:** `AddInfrastructure()` extension method

#### 4. API Layer

- **Framework:** ASP.NET Core 9 Web API
- **API Documentation:** Scalar (replaced default Swagger)
- **Configuration:** Connection strings in appsettings (Development.json gitignored)

#### 5. Database

- Initial migration created and applied
- All tables created in PostgreSQL:
  - `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc. (Identity)
  - `ai_providers`, `groups`, `group_members`, `messages`
  - `ai_response_metadata`, `refresh_tokens`

#### 6. Documentation

- Root `README.md` - project overview, setup instructions
- `src/AiGroupChat.Domain/README.md` - entity documentation
- `src/AiGroupChat.Infrastructure/README.md` - database documentation
- `.gitignore` configured for .NET, secrets, macOS

#### 7. Docker Setup

- `docker-compose.yml` for PostgreSQL development database
- Container name: `aigroupchat-db`
- Database: `aigroupchat_dev`
- Credentials: `aigroupchat` / `devpassword123`

---

## Project Structure (Current)

```
AiGroupChat/
├── src/
│   ├── AiGroupChat.API/
│   │   ├── Program.cs                 # Entry point, DI setup
│   │   ├── appsettings.json           # Base config (committed)
│   │   ├── appsettings.Development.json # Local config (gitignored)
│   │   └── AiGroupChat.API.csproj
│   │
│   ├── AiGroupChat.Application/
│   │   └── AiGroupChat.Application.csproj  # Empty, ready for services
│   │
│   ├── AiGroupChat.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Group.cs
│   │   │   ├── GroupMember.cs
│   │   │   ├── Message.cs
│   │   │   ├── AiProvider.cs
│   │   │   ├── AiResponseMetadata.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   ├── SenderType.cs
│   │   │   └── GroupRole.cs
│   │   ├── Interfaces/                 # Empty, for repository interfaces
│   │   └── README.md
│   │
│   └── AiGroupChat.Infrastructure/
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Configurations/
│       │       ├── AiProviderConfiguration.cs
│       │       ├── UserConfiguration.cs
│       │       ├── RefreshTokenConfiguration.cs
│       │       ├── GroupConfiguration.cs
│       │       ├── GroupMemberConfiguration.cs
│       │       ├── MessageConfiguration.cs
│       │       └── AiResponseMetadataConfiguration.cs
│       ├── Migrations/
│       │   └── [InitialCreate migration files]
│       ├── DependencyInjection.cs
│       └── README.md
│
├── tests/                              # Empty, ready for test projects
├── docs/
│   └── DEVELOPMENT_PROGRESS.md         # This file
├── docker-compose.yml
├── .gitignore
├── README.md
└── AiGroupChat.sln
```

---

## NuGet Packages Installed

### AiGroupChat.Domain

- `Microsoft.Extensions.Identity.Stores` (9.0.0) - For IdentityUser base class

### AiGroupChat.Infrastructure

- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.4) - PostgreSQL provider
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (9.0.0) - Identity with EF
- `Microsoft.EntityFrameworkCore.Tools` (9.0.0) - Migration tooling

### AiGroupChat.API

- `Scalar.AspNetCore` (2.11.1) - API documentation UI
- `Microsoft.EntityFrameworkCore.Design` (9.0.0) - EF design-time tools

---

## Commands Reference

```bash
# Start PostgreSQL
docker compose up -d

# Stop PostgreSQL
docker compose down

# Run the API
dotnet run --project src/AiGroupChat.API

# Build solution
dotnet build

# Add migration
dotnet ef migrations add <Name> --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Apply migrations
dotnet ef database update --project src/AiGroupChat.Infrastructure --startup-project src/AiGroupChat.API

# Access PostgreSQL CLI
docker exec -it aigroupchat-db psql -U aigroupchat -d aigroupchat_dev
```

---

## What's Next

### Phase 2: Authentication (Recommended Next)

Set up ASP.NET Identity with JWT authentication.

**Tasks:**

1. Configure ASP.NET Identity in `DependencyInjection.cs`
2. Create JWT settings and token generation service
3. Create auth DTOs in Application layer:
   - `RegisterRequest`, `LoginRequest`
   - `AuthResponse` (with tokens)
   - `RefreshTokenRequest`
4. Create `AuthController` with endpoints:
   - `POST /api/auth/register`
   - `POST /api/auth/login`
   - `POST /api/auth/refresh`
   - `POST /api/auth/logout`
5. Add JWT middleware to API pipeline
6. Test authentication flow

**Files to create/modify:**

- `src/AiGroupChat.Application/DTOs/Auth/` - Request/response DTOs
- `src/AiGroupChat.Application/Interfaces/IAuthService.cs`
- `src/AiGroupChat.Application/Services/AuthService.cs` (or Infrastructure)
- `src/AiGroupChat.Infrastructure/Services/TokenService.cs`
- `src/AiGroupChat.API/Controllers/AuthController.cs`
- Update `appsettings.json` with JWT configuration

### Phase 3: Application Layer & Core Services

Build out business logic and service interfaces.

**Tasks:**

1. Create repository interfaces in Domain or Application
2. Create DTOs for Groups, Messages, Users
3. Create service interfaces:
   - `IGroupService`
   - `IMessageService`
   - `IUserService`
4. Implement services in Application layer

### Phase 4: API Controllers

Create REST endpoints per the spec.

**Endpoints to implement:**

- `UsersController` - GET /api/users/me, GET /api/users/:id, GET /api/users?search=
- `GroupsController` - CRUD for groups
- `GroupMembersController` - Add/remove members
- `MessagesController` - Send/get messages
- `AiSettingsController` - Toggle AI, configure provider

### Phase 5: SignalR Real-time

Add WebSocket support for live messaging.

**Tasks:**

1. Create `ChatHub` for real-time events
2. Implement events: MessageReceived, UserTyping, AiTyping, etc.
3. Configure SignalR in Program.cs
4. Add authentication to SignalR connections

### Phase 6: AI Integration

Connect to the Python AI service.

**Tasks:**

1. Create `IAiClientService` interface
2. Implement HTTP client for AI service
3. Add @mention detection in message processing
4. Handle AI responses and metadata storage

---

## Database Schema Reference

See the original spec document for full schema details. Key relationships:

```
User (1) ──────< (N) RefreshToken
User (1) ──────< (N) Group (created_by)
User (1) ──────< (N) GroupMember
User (1) ──────< (N) Message (sender)

Group (1) ──────< (N) GroupMember
Group (1) ──────< (N) Message
Group (N) >────── (1) AiProvider (optional)

Message (1) ──────< (1) AiResponseMetadata
Message (N) >────── (1) AiProvider (optional)

AiProvider (1) ──────< (N) AiResponseMetadata
```

---

## Environment Setup (For New Sessions)

To continue development in a new session:

1. Navigate to project: `cd ~/Projects/AiGroupChat`
2. Start database: `docker compose up -d`
3. Verify database: `docker ps` (should show aigroupchat-db)
4. Open in VS Code: `code .`
5. Build: `dotnet build`
6. Run: `dotnet run --project src/AiGroupChat.API`
7. Access Scalar docs: `http://localhost:<port>/scalar/v1`

---

## Notes & Decisions Made

1. **Port 5434** for PostgreSQL (5433 was in use by another project)
2. **Snake_case** for database table/column names (PostgreSQL convention)
3. **Scalar** chosen over Swagger for API documentation (cleaner UI)
4. **Secrets in appsettings.Development.json** (gitignored) for local dev
5. **User.Id is string** (ASP.NET Identity default) while other entities use Guid
6. **AI Service is separate** - Python FastAPI project, not part of this solution
