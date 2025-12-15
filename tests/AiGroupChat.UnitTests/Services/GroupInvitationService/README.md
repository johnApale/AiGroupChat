# GroupInvitationService Unit Tests

Unit tests for the `GroupInvitationService` which handles group invitation operations.

## Test Coverage

### InviteMembersAsyncTests

| Test                                                      | Description                                                         |
| --------------------------------------------------------- | ------------------------------------------------------------------- |
| WithValidEmails_CreatesInvitationsAndSendsEmails          | Verifies invitations are created and emails sent for valid requests |
| WithNonexistentGroup_ThrowsNotFoundException              | Verifies error when group doesn't exist                             |
| WithNonAdmin_ThrowsAuthorizationException                 | Verifies only admins can invite                                     |
| WithInvalidEmail_ReturnsFailedResult                      | Verifies invalid emails are reported in failed array                |
| WithExistingMember_ReturnsFailedResult                    | Verifies already-members are skipped                                |
| WithExistingPendingInvitation_ResendsAndUpdatesInvitation | Verifies resend updates existing invitation                         |
| NormalizesEmailToLowercase                                | Verifies email normalization                                        |

### GetPendingInvitationsAsyncTests

| Test                                         | Description                               |
| -------------------------------------------- | ----------------------------------------- |
| WithValidRequest_ReturnsInvitations          | Verifies pending invitations are returned |
| WithNoPendingInvitations_ReturnsEmptyList    | Verifies empty list when no invitations   |
| WithNonexistentGroup_ThrowsNotFoundException | Verifies error when group doesn't exist   |
| WithNonAdmin_ThrowsAuthorizationException    | Verifies only admins can view invitations |
| ReturnsCorrectInviterDisplayName             | Verifies inviter name is included         |

### RevokeInvitationAsyncTests

| Test                                                     | Description                                       |
| -------------------------------------------------------- | ------------------------------------------------- |
| WithValidRequest_RevokesInvitation                       | Verifies invitation is revoked                    |
| WithNonexistentGroup_ThrowsNotFoundException             | Verifies error when group doesn't exist           |
| WithNonAdmin_ThrowsAuthorizationException                | Verifies only admins can revoke                   |
| WithNonexistentInvitation_ThrowsNotFoundException        | Verifies error when invitation doesn't exist      |
| WithInvitationFromDifferentGroup_ThrowsNotFoundException | Verifies group/invitation mismatch is rejected    |
| WithAlreadyAcceptedInvitation_ThrowsValidationException  | Verifies accepted invitations can't be revoked    |
| WithAlreadyRevokedInvitation_ThrowsValidationException   | Verifies already-revoked invitations are rejected |

### AcceptInvitationAsyncTests

| Test                                             | Description                                        |
| ------------------------------------------------ | -------------------------------------------------- |
| WithExistingUser_AddsToGroupAndReturnsAuth       | Verifies existing users are added and logged in    |
| WithNewUser_ReturnsRequiresRegistration          | Verifies new users get registration prompt         |
| WithInvalidToken_ThrowsNotFoundException         | Verifies error for invalid tokens                  |
| WithExpiredInvitation_ThrowsValidationException  | Verifies expired invitations are rejected          |
| WithAcceptedInvitation_ThrowsValidationException | Verifies already-accepted invitations are rejected |
| WithRevokedInvitation_ThrowsValidationException  | Verifies revoked invitations are rejected          |
| WithUserAlreadyMember_ThrowsValidationException  | Verifies already-members can't accept again        |

## Running Tests

```bash
# Run all GroupInvitationService tests
dotnet test --filter "FullyQualifiedName~GroupInvitationService"

# Run specific test class
dotnet test --filter "FullyQualifiedName~InviteMembersAsyncTests"
dotnet test --filter "FullyQualifiedName~AcceptInvitationAsyncTests"
```
