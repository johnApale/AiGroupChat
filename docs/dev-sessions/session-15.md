# Session 15: Integration Tests for GroupMembers, GroupOwner, Messages, and AiProviders Controllers

**Date**: December 2024

## Overview

This session completed integration test coverage for all remaining API controllers: GroupMembersController, GroupOwnerController, MessagesController, and AiProvidersController. This brings the total integration test count from 31 to 106 tests.

## Changes Made

### 1. GroupMembersController Tests (38 tests)

Created comprehensive tests for member management operations.

#### AddMemberTests.cs (8 tests)

| Test                                        | Status Code | Description               |
| ------------------------------------------- | ----------- | ------------------------- |
| `AddMember_AsOwner_Returns201AndMember`     | 201         | Owner can add members     |
| `AddMember_AsAdmin_Returns201AndMember`     | 201         | Admin can add members     |
| `AddMember_AsMember_Returns403`             | 403         | Regular member cannot add |
| `AddMember_AsNonMember_Returns403`          | 403         | Non-member cannot add     |
| `AddMember_WithNonExistentGroup_Returns404` | 404         | Group not found           |
| `AddMember_WithNonExistentUser_Returns404`  | 404         | User to add not found     |
| `AddMember_WithExistingMember_Returns400`   | 400         | User already a member     |
| `AddMember_WithoutToken_Returns401`         | 401         | Auth required             |

#### GetMembersTests.cs (4 tests)

| Test                                         | Status Code | Description                 |
| -------------------------------------------- | ----------- | --------------------------- |
| `GetMembers_AsMember_ReturnsAllMembers`      | 200         | Any member can view members |
| `GetMembers_AsNonMember_Returns403`          | 403         | Non-member cannot view      |
| `GetMembers_WithNonExistentGroup_Returns404` | 404         | Group not found             |
| `GetMembers_WithoutToken_Returns401`         | 401         | Auth required               |

#### UpdateMemberRoleTests.cs (10 tests)

| Test                                                     | Status Code | Description                |
| -------------------------------------------------------- | ----------- | -------------------------- |
| `UpdateMemberRole_AsOwner_ToAdmin_ReturnsUpdatedMember`  | 200         | Owner can promote to Admin |
| `UpdateMemberRole_AsOwner_ToMember_ReturnsUpdatedMember` | 200         | Owner can demote to Member |
| `UpdateMemberRole_AsAdmin_Returns403`                    | 403         | Admin cannot change roles  |
| `UpdateMemberRole_AsMember_Returns403`                   | 403         | Member cannot change roles |
| `UpdateMemberRole_ChangeOwnerRole_Returns400`            | 400         | Cannot change owner's role |
| `UpdateMemberRole_ToOwner_Returns400`                    | 400         | Cannot set role to Owner   |
| `UpdateMemberRole_WithInvalidRole_Returns400`            | 400         | Invalid role rejected      |
| `UpdateMemberRole_WithNonExistentGroup_Returns404`       | 404         | Group not found            |
| `UpdateMemberRole_WithNonExistentMember_Returns404`      | 404         | Member not found           |
| `UpdateMemberRole_WithoutToken_Returns401`               | 401         | Auth required              |

#### RemoveMemberTests.cs (10 tests)

| Test                                               | Status Code | Description                      |
| -------------------------------------------------- | ----------- | -------------------------------- |
| `RemoveMember_AsOwner_RemoveMember_Returns204`     | 204         | Owner can remove members         |
| `RemoveMember_AsOwner_RemoveAdmin_Returns204`      | 204         | Owner can remove admins          |
| `RemoveMember_AsAdmin_RemoveMember_Returns204`     | 204         | Admin can remove members         |
| `RemoveMember_AsAdmin_RemoveOtherAdmin_Returns403` | 403         | Admin cannot remove other admins |
| `RemoveMember_AsMember_Returns403`                 | 403         | Member cannot remove anyone      |
| `RemoveMember_RemoveOwner_Returns400`              | 400         | Cannot remove the owner          |
| `RemoveMember_AsNonMember_Returns403`              | 403         | Non-member cannot remove         |
| `RemoveMember_WithNonExistentGroup_Returns404`     | 404         | Group not found                  |
| `RemoveMember_WithNonExistentMember_Returns404`    | 404         | Member not found                 |
| `RemoveMember_WithoutToken_Returns401`             | 401         | Auth required                    |

#### LeaveGroupTests.cs (6 tests)

| Test                                         | Status Code | Description             |
| -------------------------------------------- | ----------- | ----------------------- |
| `LeaveGroup_AsMember_Returns204`             | 204         | Member can leave        |
| `LeaveGroup_AsAdmin_Returns204`              | 204         | Admin can leave         |
| `LeaveGroup_AsOwner_Returns400`              | 400         | Owner cannot leave      |
| `LeaveGroup_AsNonMember_Returns403`          | 403         | Non-member cannot leave |
| `LeaveGroup_WithNonExistentGroup_Returns404` | 404         | Group not found         |
| `LeaveGroup_WithoutToken_Returns401`         | 401         | Auth required           |

### 2. GroupOwnerController Tests (9 tests)

Created tests for ownership transfer operations.

#### TransferOwnershipTests.cs (9 tests)

| Test                                                | Status Code | Description                  |
| --------------------------------------------------- | ----------- | ---------------------------- |
| `TransferOwnership_AsOwner_ToMember_Returns200`     | 200         | Owner can transfer to member |
| `TransferOwnership_AsOwner_ToAdmin_Returns200`      | 200         | Owner can transfer to admin  |
| `TransferOwnership_AsAdmin_Returns403`              | 403         | Admin cannot transfer        |
| `TransferOwnership_AsMember_Returns403`             | 403         | Member cannot transfer       |
| `TransferOwnership_AsNonMember_Returns403`          | 403         | Non-member cannot transfer   |
| `TransferOwnership_ToSelf_Returns400`               | 400         | Cannot transfer to yourself  |
| `TransferOwnership_ToNonMember_Returns404`          | 404         | New owner must be a member   |
| `TransferOwnership_WithNonExistentGroup_Returns404` | 404         | Group not found              |
| `TransferOwnership_WithoutToken_Returns401`         | 401         | Auth required                |

### 3. MessagesController Tests (16 tests)

Created tests for message send and retrieve operations.

#### SendMessageTests.cs (7 tests)

| Test                                          | Status Code | Description             |
| --------------------------------------------- | ----------- | ----------------------- |
| `SendMessage_AsMember_Returns201`             | 201         | Owner can send message  |
| `SendMessage_AsRegularMember_Returns201`      | 201         | Regular member can send |
| `SendMessage_AsNonMember_Returns403`          | 403         | Non-member cannot send  |
| `SendMessage_WithEmptyContent_Returns400`     | 400         | Content required        |
| `SendMessage_WithTooLongContent_Returns400`   | 400         | Content max 10000 chars |
| `SendMessage_WithNonExistentGroup_Returns404` | 404         | Group not found         |
| `SendMessage_WithoutToken_Returns401`         | 401         | Auth required           |

#### GetMessagesTests.cs (9 tests)

| Test                                                  | Status Code | Description                    |
| ----------------------------------------------------- | ----------- | ------------------------------ |
| `GetMessages_AsMember_ReturnsMessages`                | 200         | Owner can view messages        |
| `GetMessages_AsRegularMember_ReturnsMessages`         | 200         | Regular member can view        |
| `GetMessages_WithPagination_ReturnsCorrectPage`       | 200         | Page 1 pagination works        |
| `GetMessages_WithPagination_Page2_ReturnsCorrectPage` | 200         | Page 2 pagination works        |
| `GetMessages_EmptyGroup_ReturnsEmptyList`             | 200         | Empty group returns empty list |
| `GetMessages_AsNonMember_Returns403`                  | 403         | Non-member cannot view         |
| `GetMessages_WithNonExistentGroup_Returns404`         | 404         | Group not found                |
| `GetMessages_WithoutToken_Returns401`                 | 401         | Auth required                  |

### 4. AiProvidersController Tests (6 tests)

Created tests for AI provider listing operations.

#### GetAllProvidersTests.cs (3 tests)

| Test                              | Status Code | Description                     |
| --------------------------------- | ----------- | ------------------------------- |
| `GetAll_ReturnsProviders`         | 200         | Returns list of providers       |
| `GetAll_ReturnsExpectedProviders` | 200         | Verifies seeded providers exist |
| `GetAll_WithoutToken_Returns401`  | 401         | Auth required                   |

#### GetProviderByIdTests.cs (3 tests)

| Test                                   | Status Code | Description              |
| -------------------------------------- | ----------- | ------------------------ |
| `GetById_WithValidId_ReturnsProvider`  | 200         | Returns provider details |
| `GetById_WithNonExistentId_Returns404` | 404         | Provider not found       |
| `GetById_WithoutToken_Returns401`      | 401         | Auth required            |

### 5. New Helpers Created

#### GroupMemberHelper.cs

- `AddMemberRawAsync()` / `AddMemberAsync()` - Add member to group
- `GetMembersRawAsync()` / `GetMembersAsync()` - List group members
- `UpdateMemberRoleRawAsync()` / `UpdateMemberRoleAsync()` - Update member role
- `RemoveMemberRawAsync()` - Remove member from group
- `LeaveGroupRawAsync()` - Leave group (current user)
- `TransferOwnershipRawAsync()` / `TransferOwnershipAsync()` - Transfer ownership

#### MessageHelper.cs

- `SendMessageRawAsync()` / `SendMessageAsync()` - Send message to group
- `GetMessagesRawAsync()` / `GetMessagesAsync()` - Get messages (paginated)

#### AiProviderHelper.cs

- `GetAllRawAsync()` / `GetAllAsync()` - List all AI providers
- `GetByIdRawAsync()` / `GetByIdAsync()` - Get provider by ID

### 6. Infrastructure Updates

Updated `IntegrationTestBase.cs` to include all new helpers:

- `Members` - GroupMemberHelper
- `Messages` - MessageHelper
- `AiProviders` - AiProviderHelper

### 7. Documentation

Created README files for each test directory:

- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupOwner/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/Messages/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/AiProviders/README.md`

Updated main integration tests README with complete coverage.

## Test Summary

| Controller        | Tests Added | Total |
| ----------------- | ----------- | ----- |
| GroupMembers      | 38          | 38    |
| GroupOwner        | 9           | 9     |
| Messages          | 16          | 16    |
| AiProviders       | 6           | 6     |
| **Session Total** | **69**      |       |

### Complete Test Coverage

| Controller   | Tests   |
| ------------ | ------- |
| Auth         | 6       |
| Users        | 6       |
| Groups       | 25      |
| GroupMembers | 38      |
| GroupOwner   | 9       |
| Messages     | 16      |
| AiProviders  | 6       |
| **Total**    | **106** |

## Files Created

- `tests/AiGroupChat.IntegrationTests/Helpers/GroupMemberHelper.cs`
- `tests/AiGroupChat.IntegrationTests/Helpers/MessageHelper.cs`
- `tests/AiGroupChat.IntegrationTests/Helpers/AiProviderHelper.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/AddMemberTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/GetMembersTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/UpdateMemberRoleTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/RemoveMemberTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/LeaveGroupTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupMembers/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupOwner/TransferOwnershipTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/GroupOwner/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/Messages/SendMessageTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Messages/GetMessagesTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Messages/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/AiProviders/GetAllProvidersTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/AiProviders/GetProviderByIdTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/AiProviders/README.md`
- `docs/dev-sessions/session-15.md`

## Files Modified

- `tests/AiGroupChat.IntegrationTests/Infrastructure/IntegrationTestBase.cs`
- `tests/AiGroupChat.IntegrationTests/README.md`

## Running the Tests

```bash
# Run all integration tests
dotnet test tests/AiGroupChat.IntegrationTests

# Run tests by controller
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.GroupMembers"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.GroupOwner"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Messages"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.AiProviders"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --verbosity normal
```

## Next Steps

1. **SignalR Integration Tests** - Test real-time WebSocket functionality (JoinGroup, LeaveGroup, typing indicators, message broadcasts)
2. **AI Message Integration** - Implement AI message handling from the client side
