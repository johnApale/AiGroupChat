# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 7: AI Providers Feature

**Date:** December 7, 2025

### Completed

#### 1. AiProvider Entity Updates

Added `SortOrder` field for controlling display order of providers.

**Files modified:**

- `src/AiGroupChat.Domain/Entities/AiProvider.cs` - Added `SortOrder` property
- `src/AiGroupChat.Infrastructure/Data/Configurations/AiProviderConfiguration.cs` - Added column mapping

#### 2. Group Entity - Required AiProviderId

Made `AiProviderId` required on groups (previously nullable).

**Changes:**

- `AiProviderId` changed from `Guid?` to `Guid`
- `AiProvider` navigation changed from nullable to required
- FK delete behavior changed from `SetNull` to `Restrict`
- Groups now auto-assigned the default provider (first enabled by sort order) on creation

**Files modified:**

- `src/AiGroupChat.Domain/Entities/Group.cs` - Made `AiProviderId` required
- `src/AiGroupChat.Infrastructure/Data/Configurations/GroupConfiguration.cs` - Updated FK config
- `src/AiGroupChat.Application/Services/GroupService.cs` - Inject `IAiProviderRepository`, assign default provider

#### 3. AI Provider Repository

Created repository for AI provider data access.

| Method               | Purpose                                           |
| -------------------- | ------------------------------------------------- |
| `GetAllEnabledAsync` | List enabled providers ordered by SortOrder, Name |
| `GetByIdAsync`       | Get provider by ID                                |
| `GetDefaultAsync`    | Get first enabled provider (for group creation)   |

**Files created:**

- `src/AiGroupChat.Application/Interfaces/IAiProviderRepository.cs`
- `src/AiGroupChat.Infrastructure/Repositories/AiProviderRepository.cs`

#### 4. AI Provider Service & Controller

Created service and API endpoints for listing providers.

| Method | Endpoint                | Description                | Auth |
| ------ | ----------------------- | -------------------------- | ---- |
| GET    | `/api/ai-providers`     | List all enabled providers | Yes  |
| GET    | `/api/ai-providers/:id` | Get provider by ID         | Yes  |

**Files created:**

- `src/AiGroupChat.Application/DTOs/AiProviders/AiProviderResponse.cs`
- `src/AiGroupChat.Application/Interfaces/IAiProviderService.cs`
- `src/AiGroupChat.Application/Services/AiProviderService.cs`
- `src/AiGroupChat.API/Controllers/AiProvidersController.cs`

**Files modified:**

- `src/AiGroupChat.Application/DependencyInjection.cs` - Registered `AiProviderService`
- `src/AiGroupChat.Infrastructure/DependencyInjection.cs` - Registered `AiProviderRepository`

#### 5. Seed Data

Seeded 4 default AI providers via migration:

| Name   | Display Name     | Model                      | Sort Order |
| ------ | ---------------- | -------------------------- | ---------- |
| gemini | Google Gemini    | gemini-1.5-pro             | 0          |
| claude | Anthropic Claude | claude-3-5-sonnet-20241022 | 1          |
| openai | OpenAI           | gpt-4o                     | 2          |
| grok   | xAI Grok         | grok-2                     | 3          |

#### 6. Unit Tests

Updated and added unit tests:

| File                           | Tests | Changes                                                     |
| ------------------------------ | ----- | ----------------------------------------------------------- |
| `GroupServiceTestBase.cs`      | -     | Added `IAiProviderRepository` mock, `DefaultAiProvider`     |
| `CreateAsyncTests.cs`          | 4     | Added 2 tests: assigns default provider, no providers error |
| `AiProviderServiceTestBase.cs` | -     | New test base with test providers                           |
| `GetAllAsyncTests.cs`          | 3     | List providers, empty list, DTO mapping                     |
| `GetByIdAsyncTests.cs`         | 3     | Valid ID, nonexistent ID, disabled provider                 |

**Test totals:** 81 total (75 previous + 6 new)

#### 7. Migrations

| Migration                  | Description                                 |
| -------------------------- | ------------------------------------------- |
| `AddSortOrderToAiProvider` | Added `sort_order` column to `ai_providers` |
| `MakeAiProviderIdRequired` | Made `ai_provider_id` NOT NULL on `groups`  |
| `SeedAiProviders`          | Inserted 4 default AI providers             |

#### 8. Documentation Updates

- `src/AiGroupChat.Domain/README.md` - Added AiProvider fields table
- `src/AiGroupChat.Application/README.md` - Added DTOs, interfaces, services
- `src/AiGroupChat.Infrastructure/README.md` - Added repository, updated DI list
- `src/AiGroupChat.API/README.md` - Added controller, endpoints
- `tests/AiGroupChat.UnitTests/Services/GroupService/README.md` - Updated test counts
- `tests/AiGroupChat.UnitTests/Services/AiProviderService/README.md` - New test docs

---

## What's Next: Session 8

### Option A: AI Settings Endpoints

Configure AI for groups:

| Method | Endpoint             | Description                           | Auth        |
| ------ | -------------------- | ------------------------------------- | ----------- |
| PUT    | `/api/groups/:id/ai` | Toggle AI monitoring, change provider | Owner/Admin |
| GET    | `/api/groups/:id/ai` | Get AI settings                       | Any member  |

### Option B: Messages Endpoints

Send and retrieve messages:

| Method | Endpoint                   | Description             | Auth       |
| ------ | -------------------------- | ----------------------- | ---------- |
| POST   | `/api/groups/:id/messages` | Send message            | Any member |
| GET    | `/api/groups/:id/messages` | Get history (paginated) | Any member |

### Option C: SignalR Real-time

Add WebSocket support for live messaging:

- Create `ChatHub` for real-time events
- Implement events: MessageReceived, UserTyping, MemberJoined, etc.
- Configure SignalR in Program.cs

---

## Commands Reference

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run AiProviderService tests
dotnet test --filter "FullyQualifiedName~AiProviderService"

# Run GroupService tests
dotnet test --filter "FullyQualifiedName~GroupService"

# Start PostgreSQL
docker compose up -d

# Run the API
dotnet run --project src/AiGroupChat.API

# Build solution
dotnet build
```

---

## Notes & Decisions Made

### Session 7

1. **SortOrder allows duplicates** - Easier management; ties broken by Name. Use gaps (10, 20, 30) for easier inserts.

2. **AiProviderId required** - Every group must have an AI provider assigned. Default provider auto-assigned on creation.

3. **Restrict delete on AiProvider** - Cannot delete a provider if groups are using it. Simplest approach for MVP.

4. **AiProviderResponse excludes cost fields** - Token costs (`InputTokenCost`, `OutputTokenCost`) not exposed to clients.

5. **GetByIdAsync returns 404 for disabled providers** - Disabled providers treated as non-existent from client perspective.

6. **Fixed provider IDs in seed** - Using predictable GUIDs (11111111-..., 22222222-...) for easier testing and references.
