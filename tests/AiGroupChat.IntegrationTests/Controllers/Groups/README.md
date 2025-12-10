# Groups Controller Integration Tests

## Overview

Integration tests for the `GroupsController` endpoints that handle group CRUD operations and AI settings.

## Endpoints Tested

| Method | Endpoint              | Description                  |
| ------ | --------------------- | ---------------------------- |
| POST   | `/api/groups`         | Create a new group           |
| GET    | `/api/groups`         | List groups for current user |
| GET    | `/api/groups/{id}`    | Get group by ID              |
| PUT    | `/api/groups/{id}`    | Update group                 |
| DELETE | `/api/groups/{id}`    | Delete group                 |
| PUT    | `/api/groups/{id}/ai` | Update AI settings           |

## Test Coverage

### CreateGroupTests.cs (4 tests)

| Test                                         | Status Code | Description                       |
| -------------------------------------------- | ----------- | --------------------------------- |
| `Create_WithValidRequest_Returns201AndGroup` | 201         | Creates group, user becomes owner |
| `Create_WithEmptyName_Returns400`            | 400         | Validation - name required        |
| `Create_WithTooLongName_Returns400`          | 400         | Validation - name max 200 chars   |
| `Create_WithoutToken_Returns401`             | 401         | Auth required                     |

### GetMyGroupsTests.cs (3 tests)

| Test                                        | Status Code | Description                           |
| ------------------------------------------- | ----------- | ------------------------------------- |
| `GetMyGroups_WithNoGroups_ReturnsEmptyList` | 200         | New user with no groups               |
| `GetMyGroups_ReturnsOnlyUserGroups`         | 200         | Returns only groups user is member of |
| `GetMyGroups_WithoutToken_Returns401`       | 401         | Auth required                         |

### GetGroupByIdTests.cs (4 tests)

| Test                                   | Status Code | Description            |
| -------------------------------------- | ----------- | ---------------------- |
| `GetById_AsMember_ReturnsGroup`        | 200         | Member can view group  |
| `GetById_AsNonMember_Returns403`       | 403         | Non-member cannot view |
| `GetById_WithNonExistentId_Returns404` | 404         | Group not found        |
| `GetById_WithoutToken_Returns401`      | 401         | Auth required          |

### UpdateGroupTests.cs (5 tests)

| Test                                  | Status Code | Description                |
| ------------------------------------- | ----------- | -------------------------- |
| `Update_AsOwner_ReturnsUpdatedGroup`  | 200         | Owner can update           |
| `Update_AsNonMember_Returns403`       | 403         | Non-member cannot update   |
| `Update_WithNonExistentId_Returns404` | 404         | Group not found            |
| `Update_WithEmptyName_Returns400`     | 400         | Validation - name required |
| `Update_WithoutToken_Returns401`      | 401         | Auth required              |

### DeleteGroupTests.cs (4 tests)

| Test                                  | Status Code | Description              |
| ------------------------------------- | ----------- | ------------------------ |
| `Delete_AsOwner_Returns204`           | 204         | Owner can delete         |
| `Delete_AsNonMember_Returns403`       | 403         | Non-member cannot delete |
| `Delete_WithNonExistentId_Returns404` | 404         | Group not found          |
| `Delete_WithoutToken_Returns401`      | 401         | Auth required            |

### UpdateAiSettingsTests.cs (5 tests)

| Test                                                 | Status Code | Description                  |
| ---------------------------------------------------- | ----------- | ---------------------------- |
| `UpdateAiSettings_AsOwner_ReturnsUpdatedGroup`       | 200         | Owner can update AI settings |
| `UpdateAiSettings_AsNonMember_Returns403`            | 403         | Non-member cannot update     |
| `UpdateAiSettings_WithInvalidProviderId_Returns400`  | 400         | Invalid provider ID rejected |
| `UpdateAiSettings_WithNonExistentGroupId_Returns404` | 404         | Group not found              |
| `UpdateAiSettings_WithoutToken_Returns401`           | 401         | Auth required                |

## Authorization Rules

| Action             | Owner | Admin | Member | Non-Member |
| ------------------ | ----- | ----- | ------ | ---------- |
| View group         | ✅    | ✅    | ✅     | ❌         |
| Update group       | ✅    | ✅    | ❌     | ❌         |
| Delete group       | ✅    | ❌    | ❌     | ❌         |
| Update AI settings | ✅    | ✅    | ❌     | ❌         |

## Running the Tests

```bash
# Run all Groups controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Groups"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~CreateGroupTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetMyGroupsTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetGroupByIdTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~UpdateGroupTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~DeleteGroupTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~UpdateAiSettingsTests"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.Groups" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- Group creator automatically becomes the Owner
- AI monitoring is disabled by default when a group is created
- A default AI provider is assigned when a group is created
