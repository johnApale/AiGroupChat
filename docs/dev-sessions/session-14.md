# Session 14: Integration Tests for Users and Groups Controllers

**Date**: December 9, 2024

## Overview

This session added comprehensive integration tests for the UsersController and GroupsController, following the test patterns established in session 13 for auth tests.

## Changes Made

### 1. UsersController Tests

Created `tests/AiGroupChat.IntegrationTests/Controllers/Users/UsersControllerTests.cs` with 6 tests covering:

| Test                                            | Endpoint            | Status Code | Description          |
| ----------------------------------------------- | ------------------- | ----------- | -------------------- |
| `GetCurrentUser_WithValidToken_ReturnsUserInfo` | GET /api/users/me   | 200         | Happy path           |
| `GetCurrentUser_WithoutToken_Returns401`        | GET /api/users/me   | 401         | Auth required        |
| `GetCurrentUser_WithInvalidToken_Returns401`    | GET /api/users/me   | 401         | Invalid JWT rejected |
| `GetById_WithValidId_ReturnsUserInfo`           | GET /api/users/{id} | 200         | Happy path           |
| `GetById_WithNonExistentId_Returns404`          | GET /api/users/{id} | 404         | User not found       |
| `GetById_WithoutToken_Returns401`               | GET /api/users/{id} | 401         | Auth required        |

### 2. GroupsController Tests

Created 6 test files with 25 tests covering all GroupsController endpoints:

#### CreateGroupTests.cs (4 tests)

| Test                                         | Status Code | Description                       |
| -------------------------------------------- | ----------- | --------------------------------- |
| `Create_WithValidRequest_Returns201AndGroup` | 201         | Creates group, user becomes owner |
| `Create_WithEmptyName_Returns400`            | 400         | Validation - name required        |
| `Create_WithTooLongName_Returns400`          | 400         | Validation - name max 200 chars   |
| `Create_WithoutToken_Returns401`             | 401         | Auth required                     |

#### GetMyGroupsTests.cs (3 tests)

| Test                                        | Status Code | Description                |
| ------------------------------------------- | ----------- | -------------------------- |
| `GetMyGroups_WithNoGroups_ReturnsEmptyList` | 200         | New user with no groups    |
| `GetMyGroups_ReturnsOnlyUserGroups`         | 200         | Returns only user's groups |
| `GetMyGroups_WithoutToken_Returns401`       | 401         | Auth required              |

#### GetGroupByIdTests.cs (4 tests)

| Test                                   | Status Code | Description            |
| -------------------------------------- | ----------- | ---------------------- |
| `GetById_AsMember_ReturnsGroup`        | 200         | Member can view group  |
| `GetById_AsNonMember_Returns403`       | 403         | Non-member cannot view |
| `GetById_WithNonExistentId_Returns404` | 404         | Group not found        |
| `GetById_WithoutToken_Returns401`      | 401         | Auth required          |

#### UpdateGroupTests.cs (5 tests)

| Test                                  | Status Code | Description                |
| ------------------------------------- | ----------- | -------------------------- |
| `Update_AsOwner_ReturnsUpdatedGroup`  | 200         | Owner can update           |
| `Update_AsNonMember_Returns403`       | 403         | Non-member cannot update   |
| `Update_WithNonExistentId_Returns404` | 404         | Group not found            |
| `Update_WithEmptyName_Returns400`     | 400         | Validation - name required |
| `Update_WithoutToken_Returns401`      | 401         | Auth required              |

#### DeleteGroupTests.cs (4 tests)

| Test                                  | Status Code | Description              |
| ------------------------------------- | ----------- | ------------------------ |
| `Delete_AsOwner_Returns204`           | 204         | Owner can delete         |
| `Delete_AsNonMember_Returns403`       | 403         | Non-member cannot delete |
| `Delete_WithNonExistentId_Returns404` | 404         | Group not found          |
| `Delete_WithoutToken_Returns401`      | 401         | Auth required            |

#### UpdateAiSettingsTests.cs (5 tests)

| Test                                                 | Status Code | Description                  |
| ---------------------------------------------------- | ----------- | ---------------------------- |
| `UpdateAiSettings_AsOwner_ReturnsUpdatedGroup`       | 200         | Owner can update AI settings |
| `UpdateAiSettings_AsNonMember_Returns403`            | 403         | Non-member cannot update     |
| `UpdateAiSettings_WithInvalidProviderId_Returns400`  | 400         | Invalid provider rejected    |
| `UpdateAiSettings_WithNonExistentGroupId_Returns404` | 404         | Group not found              |
| `UpdateAiSettings_WithoutToken_Returns401`           | 401         | Auth required                |

### 3. Infrastructure Updates

#### Added GroupHelper

Created `tests/AiGroupChat.IntegrationTests/Helpers/GroupHelper.cs` with helper methods:

- `CreateGroupAsync()` - Creates a group and returns the response
- `CreateGroupRawAsync()` - Creates a group and returns HTTP response
- `GetGroupRawAsync()` - Gets a group by ID
- `GetMyGroupsRawAsync()` - Gets all groups for current user
- `UpdateGroupRawAsync()` - Updates a group
- `DeleteGroupRawAsync()` - Deletes a group
- `UpdateAiSettingsRawAsync()` - Updates AI settings

#### Updated IntegrationTestBase

Added `GroupHelper` to the base class so all tests can access it via `Groups` property.

### 4. Documentation

Created README files for both test directories:

- `tests/AiGroupChat.IntegrationTests/Controllers/Users/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/README.md`

## Test Summary

| Controller       | Tests  |
| ---------------- | ------ |
| UsersController  | 6      |
| GroupsController | 25     |
| **Total**        | **31** |

## Files Created

- `tests/AiGroupChat.IntegrationTests/Controllers/Users/UsersControllerTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Users/README.md`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/CreateGroupTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/GetMyGroupsTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/GetGroupByIdTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/UpdateGroupTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/DeleteGroupTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/UpdateAiSettingsTests.cs`
- `tests/AiGroupChat.IntegrationTests/Controllers/Groups/README.md`
- `tests/AiGroupChat.IntegrationTests/Helpers/GroupHelper.cs`
- `docs/dev-sessions/session-14.md`

## Files Modified

- `tests/AiGroupChat.IntegrationTests/Infrastructure/IntegrationTestBase.cs`

## Running the Tests

```bash
# Run all Users controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Users"

# Run all Groups controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Groups"

# Run all integration tests
dotnet test tests/AiGroupChat.IntegrationTests
```

## Next Steps

The following controller tests could be added next:

1. **GroupMembersController** - Member management (add, remove, update role)
2. **GroupOwnerController** - Ownership transfer
3. **MessagesController** - Message sending/retrieval
4. **AiProvidersController** - AI provider listing
