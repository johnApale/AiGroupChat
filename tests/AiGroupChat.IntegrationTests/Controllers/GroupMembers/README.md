# Group Members Controller Integration Tests

## Overview

Integration tests for the `GroupMembersController` endpoints that handle group membership operations including adding/removing members, updating roles, and leaving groups.

## Endpoints Tested

| Method | Endpoint                             | Description         |
| ------ | ------------------------------------ | ------------------- |
| POST   | `/api/groups/{groupId}/members`      | Add member to group |
| GET    | `/api/groups/{groupId}/members`      | List group members  |
| PUT    | `/api/groups/{groupId}/members/{id}` | Update member role  |
| DELETE | `/api/groups/{groupId}/members/{id}` | Remove member       |
| DELETE | `/api/groups/{groupId}/members/me`   | Leave group         |

## Test Coverage

### AddMemberTests.cs (8 tests)

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

### GetMembersTests.cs (4 tests)

| Test                                         | Status Code | Description                 |
| -------------------------------------------- | ----------- | --------------------------- |
| `GetMembers_AsMember_ReturnsAllMembers`      | 200         | Any member can view members |
| `GetMembers_AsNonMember_Returns403`          | 403         | Non-member cannot view      |
| `GetMembers_WithNonExistentGroup_Returns404` | 404         | Group not found             |
| `GetMembers_WithoutToken_Returns401`         | 401         | Auth required               |

### UpdateMemberRoleTests.cs (10 tests)

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

### RemoveMemberTests.cs (10 tests)

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

### LeaveGroupTests.cs (6 tests)

| Test                                         | Status Code | Description             |
| -------------------------------------------- | ----------- | ----------------------- |
| `LeaveGroup_AsMember_Returns204`             | 204         | Member can leave        |
| `LeaveGroup_AsAdmin_Returns204`              | 204         | Admin can leave         |
| `LeaveGroup_AsOwner_Returns400`              | 400         | Owner cannot leave      |
| `LeaveGroup_AsNonMember_Returns403`          | 403         | Non-member cannot leave |
| `LeaveGroup_WithNonExistentGroup_Returns404` | 404         | Group not found         |
| `LeaveGroup_WithoutToken_Returns401`         | 401         | Auth required           |

## Authorization Rules

### Add Member

| Role       | Can Add |
| ---------- | ------- |
| Owner      | ✅      |
| Admin      | ✅      |
| Member     | ❌      |
| Non-Member | ❌      |

### View Members

| Role       | Can View |
| ---------- | -------- |
| Owner      | ✅       |
| Admin      | ✅       |
| Member     | ✅       |
| Non-Member | ❌       |

### Update Role (Owner only)

| Role       | Can Update |
| ---------- | ---------- |
| Owner      | ✅         |
| Admin      | ❌         |
| Member     | ❌         |
| Non-Member | ❌         |

### Remove Member

| Actor      | Can Remove Owner | Can Remove Admin | Can Remove Member |
| ---------- | ---------------- | ---------------- | ----------------- |
| Owner      | ❌               | ✅               | ✅                |
| Admin      | ❌               | ❌               | ✅                |
| Member     | ❌               | ❌               | ❌                |
| Non-Member | ❌               | ❌               | ❌                |

### Leave Group

| Role       | Can Leave |
| ---------- | --------- |
| Owner      | ❌        |
| Admin      | ✅        |
| Member     | ✅        |
| Non-Member | ❌        |

## Running the Tests

```bash
# Run all GroupMembers controller tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.GroupMembers"

# Run specific test file
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~AddMemberTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GetMembersTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~UpdateMemberRoleTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~RemoveMemberTests"
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~LeaveGroupTests"

# Run with verbose output
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~Controllers.GroupMembers" --verbosity normal
```

## Notes

- All endpoints require authentication (`[Authorize]` attribute on controller)
- New members are added with the `Member` role by default
- Only the Owner can change member roles (Admin ↔ Member)
- Owner cannot leave the group - must transfer ownership first or delete the group
- Owner cannot be removed - must transfer ownership first or delete the group
- Admins can only remove Members, not other Admins
