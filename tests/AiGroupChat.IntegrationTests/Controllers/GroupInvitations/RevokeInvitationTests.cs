using System.Net;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupInvitations;

public class RevokeInvitationTests : IntegrationTestBase
{
    public RevokeInvitationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RevokeInvitation_AsOwner_Returns204()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group.Id, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify invitation is no longer pending
        List<InvitationResponse> pending = await Invitations.GetPendingInvitationsAsync(group.Id);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task RevokeInvitation_AsAdmin_Returns204()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Create and promote admin
        Auth.ClearAuthToken();
        AuthResponse admin = await Auth.CreateAuthenticatedUserAsync(
            email: "admin@example.com",
            userName: "admin",
            displayName: "Admin User");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin.User.Id, "Admin");

        // Switch to admin
        Auth.SetAuthToken(admin.AccessToken);

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group.Id, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_AsMember_Returns403()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Create regular member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Switch to member
        Auth.SetAuthToken(member.AccessToken);

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group.Id, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(groupId, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_WithNonexistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        Guid nonExistentGroupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(nonExistentGroupId, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_WithNonexistentInvitation_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse group = await Groups.CreateGroupAsync("Test Group");
        Guid nonExistentInvitationId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group.Id, nonExistentInvitationId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_WithInvitationFromDifferentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        GroupResponse group1 = await Groups.CreateGroupAsync("Group 1");
        GroupResponse group2 = await Groups.CreateGroupAsync("Group 2");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group1.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Act - Try to revoke group1's invitation using group2's endpoint
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group2.Id, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RevokeInvitation_AlreadyRevoked_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Revoke once
        await Invitations.RevokeInvitationAsync(group.Id, invitationId);

        // Act - Try to revoke again
        HttpResponseMessage response = await Invitations.RevokeInvitationRawAsync(group.Id, invitationId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}