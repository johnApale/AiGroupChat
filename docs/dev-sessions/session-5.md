# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 5: Users & Groups Endpoints

**Date:** December 7, 2025

### Completed

#### 1. Users Endpoints

Created user profile endpoints:

| Method | Endpoint         | Description                    |
| ------ | ---------------- | ------------------------------ |
| GET    | `/api/users/me`  | Get current authenticated user |
| GET    | `/api/users/:id` | Get user by ID                 |

**Files created:**

- `src/AiGroupChat.Application/DTOs/Users/UserResponse.cs`
- `src/AiGroupChat.Application/Interfaces/IUserService.cs`
- `src/AiGroupChat.Application/Services/UserService.cs`
- `src/AiGroupChat.API/Controllers/UsersController.cs`

#### 2. Groups CRUD Endpoints

Created group management endpoints with authorization:

| Method | Endpoint          | Description                    | Auth         |
| ------ | ----------------- | ------------------------------ | ------------ |
| POST   | `/api/groups`     | Create group (become admin)    | Yes          |
| GET    | `/api/groups`     | List my groups with members    | Yes          |
| GET    | `/api/groups/:id` | Get group details with members | Yes (member) |
| PUT    | `/api/groups/:id` | Update group name              | Yes (admin)  |
| DELETE | `/api/groups/:id` | Delete group                   | Yes (admin)  |

**Business Rules Implemented:**

- Creator automatically becomes a member with Admin role
- Only group members can view group details
- Only group admins can update/delete groups
- List returns only groups where user is a member
- Group details include full members list

**Files created:**

- `src/AiGroupChat.Application/DTOs/Groups/CreateGroupRequest.cs`
- `src/AiGroupChat.Application/DTOs/Groups/UpdateGroupRequest.cs`
- `src/AiGroupChat.Application/DTOs/Groups/GroupResponse.cs`
- `src/AiGroupChat.Application/DTOs/Groups/GroupMemberResponse.cs`
- `src/AiGroupChat.Application/Interfaces/IGroupRepository.cs`
- `src/AiGroupChat.Application/Interfaces/IGroupService.cs`
- `src/AiGroupChat.Application/Services/GroupService.cs`
- `src/AiGroupChat.Application/Exceptions/AuthorizationException.cs`
- `src/AiGroupChat.Infrastructure/Repositories/GroupRepository.cs`
- `src/AiGroupChat.API/Controllers/GroupsController.cs`

#### 3. Unit Tests

Added comprehensive unit tests for new services:

**UserService Tests (4 tests):**

- `GetByIdAsyncTests` - Valid ID, nonexistent ID
- `GetCurrentUserAsyncTests` - Valid user, invalid user

**GroupService Tests (13 tests):**

- `CreateAsyncTests` - Valid creation, creator becomes admin
- `GetMyGroupsAsyncTests` - Returns groups, returns empty list
- `GetByIdAsyncTests` - Valid member, nonexistent group, non-member
- `UpdateAsyncTests` - Valid admin update, nonexistent group, non-admin
- `DeleteAsyncTests` - Valid admin delete, nonexistent group, non-admin

**Total tests: 42** (25 AuthService + 4 UserService + 13 GroupService)

#### 4. Exception Handling

Added `AuthorizationException` for 403 Forbidden responses when users lack permission to perform actions.

---

## What's Next: Session 6

### Option A: Group Members Endpoints

Add/remove/update group members:

| Method | Endpoint                          | Description        | Auth         |
| ------ | --------------------------------- | ------------------ | ------------ |
| POST   | `/api/groups/:id/members`         | Add member         | Yes (admin)  |
| GET    | `/api/groups/:id/members`         | List members       | Yes (member) |
| DELETE | `/api/groups/:id/members/:userId` | Remove member      | Yes (admin)  |
| PUT    | `/api/groups/:id/members/:userId` | Update member role | Yes (admin)  |

**Business Rules to Implement:**

- Only admins can add/remove members
- Only admins can change member roles
- Admin cannot remove themselves if they are the only admin
- Any member can view the members list

### Option B: AI Settings Endpoints

Configure AI for groups:

| Method | Endpoint             | Description         | Auth         |
| ------ | -------------------- | ------------------- | ------------ |
| PUT    | `/api/groups/:id/ai` | Toggle/configure AI | Yes (admin)  |
| GET    | `/api/groups/:id/ai` | Get AI settings     | Yes (member) |

### Option C: AI Providers Endpoints

List available AI providers:

| Method | Endpoint                | Description          | Auth |
| ------ | ----------------------- | -------------------- | ---- |
| GET    | `/api/ai-providers`     | List all providers   | Yes  |
| GET    | `/api/ai-providers/:id` | Get provider details | Yes  |

### Option D: Messages Endpoints

Send and retrieve messages:

| Method | Endpoint                   | Description             | Auth         |
| ------ | -------------------------- | ----------------------- | ------------ |
| POST   | `/api/groups/:id/messages` | Send message            | Yes (member) |
| GET    | `/api/groups/:id/messages` | Get history (paginated) | Yes (member) |

---

## Commands Reference

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific service tests
dotnet test --filter "FullyQualifiedName~GroupService"
dotnet test --filter "FullyQualifiedName~UserService"

# Start PostgreSQL
docker compose up -d

# Run the API
dotnet run --project src/AiGroupChat.API

# Build solution
dotnet build
```

---

## Notes & Decisions Made

### Session 5

1. **Users search endpoint deferred** - `GET /api/users?search=` moved to future enhancement

2. **Members included in group responses** - Group details always include the full members list for frontend flexibility

3. **Authorization via exceptions** - Created `AuthorizationException` for 403 responses, handled by global middleware

4. **Separate member endpoints planned** - Even though members are included in group responses, dedicated `/members` endpoints will be built for managing membership

5. **Admin protection rule** - Before an admin removes themselves, there must be another admin in the group (to be implemented in Group Members endpoints)
