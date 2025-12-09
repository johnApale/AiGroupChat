# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 8: AI Settings & Messages Endpoints

**Date:** December 8, 2025

### Completed

#### 1. AI Settings Endpoint

Added endpoint to update AI settings for a group (monitoring toggle and provider selection).

| Method | Endpoint             | Description         | Auth       |
| ------ | -------------------- | ------------------- | ---------- |
| PUT    | `/api/groups/:id/ai` | Update AI settings  | Yes (admin)|

**Request body (partial update - both fields optional):**
```json
{
  "aiMonitoringEnabled": true,
  "aiProviderId": "11111111-1111-1111-1111-111111111111"
}
```

**Files created:**
- `src/AiGroupChat.Application/DTOs/Groups/UpdateAiSettingsRequest.cs`

**Files modified:**
- `src/AiGroupChat.Application/Interfaces/IGroupService.cs` - Added `UpdateAiSettingsAsync`
- `src/AiGroupChat.Application/Services/GroupService.cs` - Implemented `UpdateAiSettingsAsync`
- `src/AiGroupChat.API/Controllers/GroupsController.cs` - Added PUT endpoint

#### 2. Messages Endpoints

Added endpoints for sending and retrieving messages with pagination.

| Method | Endpoint                   | Description              | Auth        |
| ------ | -------------------------- | ------------------------ | ----------- |
| POST   | `/api/groups/:id/messages` | Send message             | Yes (member)|
| GET    | `/api/groups/:id/messages` | Get messages (paginated) | Yes (member)|

**Request body (POST):**
```json
{
  "content": "Hello everyone!"
}
```

**Query params (GET):**
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 50, max: 100)

**Response (GET):**
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 50,
  "totalCount": 150,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Files created:**
- `src/AiGroupChat.Application/DTOs/Messages/SendMessageRequest.cs`
- `src/AiGroupChat.Application/DTOs/Messages/MessageResponse.cs`
- `src/AiGroupChat.Application/DTOs/Common/PaginatedResponse.cs`
- `src/AiGroupChat.Application/Interfaces/IMessageRepository.cs`
- `src/AiGroupChat.Application/Interfaces/IMessageService.cs`
- `src/AiGroupChat.Application/Services/MessageService.cs`
- `src/AiGroupChat.Infrastructure/Repositories/MessageRepository.cs`
- `src/AiGroupChat.API/Controllers/MessagesController.cs`

**Files modified:**
- `src/AiGroupChat.Application/DependencyInjection.cs` - Registered `MessageService`
- `src/AiGroupChat.Infrastructure/DependencyInjection.cs` - Registered `MessageRepository`

#### 3. Unit Tests

| File                              | Tests | Scenarios Covered                                                              |
| --------------------------------- | ----- | ------------------------------------------------------------------------------ |
| `UpdateAiSettingsAsyncTests.cs`   | 7     | Update monitoring, update provider, update both, nonexistent group, non-admin, invalid provider, empty request |
| `SendMessageAsyncTests.cs`        | 4     | Valid send, AI visible when monitoring on, nonexistent group, non-member       |
| `GetMessagesAsyncTests.cs`        | 6     | Valid retrieval, pagination, empty group, nonexistent group, non-member, page size clamping |

**Test totals:** 98 total (88 previous + 10 new)

#### 4. Documentation Updates

- `src/AiGroupChat.API/README.md` - Added Messages endpoints, MessagesController
- `src/AiGroupChat.Application/README.md` - Added DTOs, interfaces, services
- `src/AiGroupChat.Infrastructure/README.md` - Added MessageRepository
- `tests/AiGroupChat.UnitTests/Services/GroupService/README.md` - Updated test counts
- `tests/AiGroupChat.UnitTests/Services/MessageService/README.md` - New test docs

---

## What's Next: Session 9

### Option A: SignalR Real-time Messaging

Add WebSocket support for live messaging:

- Create `ChatHub` for real-time events
- Implement events: `MessageReceived`, `UserTyping`, `MemberJoined`, etc.
- Configure SignalR in `Program.cs`
- Broadcast messages when sent via REST API

### Option B: AI Integration Preparation

Prepare for AI service integration:

- Create AI service HTTP client interface
- Define request/response contracts for Python AI service
- Implement AI message sending flow
- Store AI response metadata

### Option C: Read Status & Cursor Pagination

Enhance messaging features:

- Add `message_reads` table for read tracking
- Implement cursor-based pagination
- Add unread message count endpoint

---

## Commands Reference

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run MessageService tests
dotnet test --filter "FullyQualifiedName~MessageService"

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

### Session 8

1. **No separate GET for AI settings** - `GroupResponse` already includes `AiMonitoringEnabled` and `AiProvider`, so no dedicated endpoint needed.

2. **Partial update for AI settings** - Both `AiMonitoringEnabled` and `AiProviderId` are optional in the request. Only provided fields are updated.

3. **AiVisible flag** - Messages have `AiVisible` set based on the group's `AiMonitoringEnabled` at the time of sending. This is immutable after creation.

4. **Offset-based pagination** - Using simple page/pageSize for MVP. Cursor-based pagination can be added later for infinite scroll UX.

5. **Page size clamping** - Maximum 100 items per page to prevent excessive queries. Values above 100 are clamped.

6. **Explicit types** - Using explicit type declarations instead of `var` for better code readability.
