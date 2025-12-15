# Group Invitations Integration Tests

Integration tests for the group invitation endpoints.

## Test Coverage

### InviteMembersTests

| Test                                                 | Description                                  |
| ---------------------------------------------------- | -------------------------------------------- |
| InviteMembers_AsOwner_Returns200AndSendsEmails       | Owner can invite members and emails are sent |
| InviteMembers_AsAdmin_Returns200                     | Admin can invite members                     |
| InviteMembers_AsMember_Returns403                    | Regular members cannot invite                |
| InviteMembers_WithoutToken_Returns401                | Unauthenticated requests rejected            |
| InviteMembers_WithNonexistentGroup_Returns404        | Invalid group ID returns 404                 |
| InviteMembers_WithExistingMember_ReturnsFailedResult | Already-members reported in failed array     |
| InviteMembers_WithInvalidEmail_ReturnsFailedResult   | Invalid emails reported in failed array      |
| InviteMembers_ResendingInvitation_UpdatesExisting    | Resending updates existing invitation        |

### GetPendingInvitationsTests

| Test                                                  | Description                        |
| ----------------------------------------------------- | ---------------------------------- |
| GetPendingInvitations_AsOwner_ReturnsInvitations      | Owner can view pending invitations |
| GetPendingInvitations_AsAdmin_ReturnsInvitations      | Admin can view pending invitations |
| GetPendingInvitations_AsMember_Returns403             | Regular members cannot view        |
| GetPendingInvitations_WithoutToken_Returns401         | Unauthenticated requests rejected  |
| GetPendingInvitations_WithNonexistentGroup_Returns404 | Invalid group ID returns 404       |
| GetPendingInvitations_WithNoPending_ReturnsEmptyList  | Empty list when no invitations     |
| GetPendingInvitations_ReturnsCorrectInviterInfo       | Inviter display name is included   |

### RevokeInvitationTests

| Test                                                         | Description                       |
| ------------------------------------------------------------ | --------------------------------- |
| RevokeInvitation_AsOwner_Returns204                          | Owner can revoke invitations      |
| RevokeInvitation_AsAdmin_Returns204                          | Admin can revoke invitations      |
| RevokeInvitation_AsMember_Returns403                         | Regular members cannot revoke     |
| RevokeInvitation_WithoutToken_Returns401                     | Unauthenticated requests rejected |
| RevokeInvitation_WithNonexistentGroup_Returns404             | Invalid group ID returns 404      |
| RevokeInvitation_WithNonexistentInvitation_Returns404        | Invalid invitation ID returns 404 |
| RevokeInvitation_WithInvitationFromDifferentGroup_Returns404 | Cross-group revocation rejected   |
| RevokeInvitation_AlreadyRevoked_Returns400                   | Cannot revoke twice               |

## API Endpoints Tested

| Method | Endpoint                                 | Description              |
| ------ | ---------------------------------------- | ------------------------ |
| POST   | `/api/groups/{groupId}/invitations`      | Send invitations         |
| GET    | `/api/groups/{groupId}/invitations`      | List pending invitations |
| DELETE | `/api/groups/{groupId}/invitations/{id}` | Revoke invitation        |

## Running Tests

```bash
# Run all GroupInvitations tests
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~GroupInvitations"

# Run specific test class
dotnet test tests/AiGroupChat.IntegrationTests --filter "FullyQualifiedName~InviteMembersTests"
```
