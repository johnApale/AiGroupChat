# Development Progress

This document tracks the development progress of the AI Group Chat application and serves as a handoff reference for continuing work in future sessions.

---

## Session 6: Owner Role & Group Members Endpoints

**Date:** December 7, 2025

### Completed

#### 1. Owner Role Refactor

Added `Owner` role to the group hierarchy for clearer permission management.

**Role Hierarchy:**

| Role   | Permissions                                                              |
| ------ | ------------------------------------------------------------------------ |
| Owner  | All permissions, transfer ownership, delete group, promote/demote admins |
| Admin  | Add/remove members (not admins), update group name, change AI settings   |
| Member | View group, send messages, leave group                                   |

**Files modified:**

- `src/AiGroupChat.Domain/Enums/GroupRole.cs` - Added `Owner` value
- `src/AiGroupChat.Application/Services/GroupService.cs` - Creator becomes Owner
- `src/AiGroupChat.Infrastructure/Repositories/GroupRepository.cs` - `IsAdminAsync` includes Owner, added `IsOwnerAsync`
- `src/AiGroupChat.Application/Interfaces/IGroupRepository.cs` - Added `IsOwnerAsync`
- `src/AiGroupChat.Domain/README.md` - Updated documentation

#### 2. Group Members Endpoints

Created member management endpoints:

| Method | Endpoint                          | Description        | Auth           |
| ------ | --------------------------------- | ------------------ | -------------- |
| POST   | `/api/groups/:id/members`         | Add member         | Owner/Admin    |
| GET    | `/api/groups/:id/members`         | List members       | Any member     |
| PUT    | `/api/groups/:id/members/:userId` | Update member role | Owner only     |
| DELETE | `/api/groups/:id/members/:userId` | Remove member      | Owner/Admin\*  |
| DELETE | `/api/groups/:id/members/me`      | Leave group        | Any member\*\* |
| PUT    | `/api/groups/:id/owner`           | Transfer ownership | Owner only     |

\*Admin can only remove Members, Owner can remove anyone except themselves
\*\*Owner cannot leave without transferring ownership first

**Files created:**

- `src/AiGroupChat.Application/DTOs/Groups/AddMemberRequest.cs`
- `src/AiGroupChat.Application/DTOs/Groups/UpdateMemberRoleRequest.cs`
- `src/AiGroupChat.Application/DTOs/Groups/TransferOwnershipRequest.cs`
- `src/AiGroupChat.Application/Interfaces/IGroupMemberService.cs`
- `src/AiGroupChat.Application/Services/GroupMemberService.cs`
- `src/AiGroupChat.API/Controllers/GroupMembersController.cs`
- `src/AiGroupChat.API/Controllers/GroupOwnerController.cs`

**Files modified:**

- `src/AiGroupChat.Application/Interfaces/IGroupRepository.cs` - Added member management methods
- `src/AiGroupChat.Infrastructure/Repositories/GroupRepository.cs` - Implemented new methods
- `src/AiGroupChat.Application/DependencyInjection.cs` - Registered `GroupMemberService`

#### 3. Repository Methods Added

New methods in `IGroupRepository`:

| Method              | Purpose                              |
| ------------------- | ------------------------------------ |
| `IsOwnerAsync`      | Check if user is the group owner     |
| `GetMemberAsync`    | Get a specific member with user data |
| `UpdateMemberAsync` | Update a member's role               |
| `RemoveMemberAsync` | Remove a member from a group         |
| `CountAdminsAsync`  | Count admins/owners in a group       |

#### 4. Unit Tests

Added comprehensive unit tests for `GroupMemberService`:

| File                             | Tests | Scenarios                                                                  |
| -------------------------------- | ----- | -------------------------------------------------------------------------- |
| `AddMemberAsyncTests.cs`         | 5     | Valid add, nonexistent group, non-admin, nonexistent user, existing member |
| `GetMembersAsyncTests.cs`        | 3     | Valid member, non-member, nonexistent group                                |
| `UpdateMemberRoleAsyncTests.cs`  | 7     | Promote/demote, non-owner, change owner role, invalid role, nonexistent    |
| `RemoveMemberAsyncTests.cs`      | 6     | Owner/admin removing, admin removing admin, remove owner, non-admin        |
| `LeaveGroupAsyncTests.cs`        | 5     | Member/admin leaves, owner leaves, non-member, nonexistent group           |
| `TransferOwnershipAsyncTests.cs` | 5     | Valid transfer, non-owner, non-member target, self-transfer, nonexistent   |

**Test totals:** 73 total (42 previous + 31 new)

#### 5. Updated Unit Tests for Owner Refactor

Modified existing tests to reflect Owner role:

- `CreateAsyncTests.cs` - Creator becomes Owner (not Admin)
- `DeleteAsyncTests.cs` - Only Owner can delete (not Admin)
- `GetMyGroupsAsyncTests.cs` - Updated test data roles

#### 6. Documentation Updates

- `src/AiGroupChat.Domain/README.md` - Added Owner role and hierarchy
- `src/AiGroupChat.Application/README.md` - Added `IGroupMemberService` and `GroupMemberService`
- `src/AiGroupChat.Infrastructure/README.md` - Updated repository descriptions
- `src/AiGroupChat.API/README.md` - Added new controllers and endpoints
- `tests/AiGroupChat.UnitTests/Services/GroupMemberService/README.md` - New test documentation
- `tests/AiGroupChat.UnitTests/Services/GroupService/README.md` - Updated for Owner role

---

## What's Next: Session 7

### Option A: AI Settings Endpoints

Configure AI for groups:

| Method | Endpoint             | Description         | Auth        |
| ------ | -------------------- | ------------------- | ----------- |
| PUT    | `/api/groups/:id/ai` | Toggle/configure AI | Owner/Admin |
| GET    | `/api/groups/:id/ai` | Get AI settings     | Any member  |

### Option B: AI Providers Endpoints

List available AI providers:

| Method | Endpoint                | Description          | Auth |
| ------ | ----------------------- | -------------------- | ---- |
| GET    | `/api/ai-providers`     | List all providers   | Yes  |
| GET    | `/api/ai-providers/:id` | Get provider details | Yes  |

### Option C: Messages Endpoints

Send and retrieve messages:

| Method | Endpoint                   | Description             | Auth       |
| ------ | -------------------------- | ----------------------- | ---------- |
| POST   | `/api/groups/:id/messages` | Send message            | Any member |
| GET    | `/api/groups/:id/messages` | Get history (paginated) | Any member |

### Option D: SignalR Real-time

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

# Run GroupMemberService tests
dotnet test --filter "FullyQualifiedName~GroupMemberService"

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

### Session 6

1. **Owner role added** - Three-tier hierarchy (Owner > Admin > Member) provides clearer permission boundaries

2. **Owner vs Admin permissions:**

   - Only Owner can delete the group
   - Only Owner can promote/demote admins
   - Only Owner can transfer ownership
   - Admins can add/remove regular members but not other admins

3. **Owner cannot leave** - Must transfer ownership first or delete the group

4. **Transfer ownership demotes previous owner to Admin** - Previous owner retains admin privileges after transfer

5. **Separate controllers for members and owner** - `GroupMembersController` handles member CRUD, `GroupOwnerController` handles ownership transfer

6. **`IsAdminAsync` includes Owner** - Simplifies authorization checks where "admin or higher" is needed

7. **Kept `CountAdminsAsync`** - Reserved for potential future use (e.g., preventing removal of last admin)
