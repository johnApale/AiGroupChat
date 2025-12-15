using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupInvitations;

public class InviteMembersTests : IntegrationTestBase
{
    public InviteMembersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task InviteMembers_AsOwner_Returns200AndSendsEmails()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        List<string> emails = new List<string> { "invite1@example.com", "invite2@example.com" };

        // Clear emails from user registration
        EmailProvider.Clear();

        // Act
        HttpResponseMessage response = await Invitations.InviteMembersRawAsync(group.Id, emails);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        InviteMembersResponse? result = await response.Content.ReadFromJsonAsync<InviteMembersResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Sent.Count);
        Assert.Empty(result.Failed);

        // Verify emails were sent
        Assert.Equal(2, EmailProvider.SentEmails.Count);
        Assert.Contains(EmailProvider.SentEmails, e => e.To == "invite1@example.com");
        Assert.Contains(EmailProvider.SentEmails, e => e.To == "invite2@example.com");
    }

    [Fact]
    public async Task InviteMembers_AsAdmin_Returns200()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create admin user
        Auth.ClearAuthToken();
        AuthResponse admin = await Auth.CreateAuthenticatedUserAsync(
            email: "admin@example.com",
            userName: "admin",
            displayName: "Admin User");

        // Add admin to group and promote
        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin.User.Id, "Admin");

        // Switch to admin
        Auth.SetAuthToken(admin.AccessToken);

        // Act
        HttpResponseMessage response = await Invitations.InviteMembersRawAsync(
            group.Id, new List<string> { "newuser@example.com" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InviteMembers_AsMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

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
        HttpResponseMessage response = await Invitations.InviteMembersRawAsync(
            group.Id, new List<string> { "newuser@example.com" });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InviteMembers_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();
        Guid groupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.InviteMembersRawAsync(
            groupId, new List<string> { "user@example.com" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InviteMembers_WithNonexistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.InviteMembersRawAsync(
            nonExistentGroupId, new List<string> { "user@example.com" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InviteMembers_WithExistingMember_ReturnsFailedResult()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create and add a member
        Auth.ClearAuthToken();
        AuthResponse existingMember = await Auth.CreateAuthenticatedUserAsync(
            email: "existing@example.com",
            userName: "existing",
            displayName: "Existing Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, existingMember.User.Id);

        // Act - Try to invite the existing member
        InviteMembersResponse result = await Invitations.InviteMembersAsync(
            group.Id, new List<string> { "existing@example.com" });

        // Assert
        Assert.Empty(result.Sent);
        Assert.Single(result.Failed);
        Assert.Equal("existing@example.com", result.Failed[0].Email);
        Assert.Contains("already a member", result.Failed[0].Reason.ToLower());
    }

    [Fact]
    public async Task InviteMembers_WithInvalidEmail_ReturnsFailedResult()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act
        InviteMembersResponse result = await Invitations.InviteMembersAsync(
            group.Id, new List<string> { "invalid-email", "valid@example.com" });

        // Assert
        Assert.Single(result.Sent);
        Assert.Single(result.Failed);
        Assert.Equal("invalid-email", result.Failed[0].Email);
    }

    [Fact]
    public async Task InviteMembers_ResendingInvitation_UpdatesExisting()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        string email = "resend@example.com";

        // Send first invitation
        await Invitations.InviteMemberAsync(group.Id, email);

        // Act - Send again
        InviteMembersResponse result = await Invitations.InviteMembersAsync(
            group.Id, new List<string> { email });

        // Assert
        Assert.Single(result.Sent);
        Assert.Empty(result.Failed);
        Assert.Equal(2, result.Sent[0].SendCount);

        // Verify only one pending invitation exists
        List<InvitationResponse> pending = await Invitations.GetPendingInvitationsAsync(group.Id);
        Assert.Single(pending);
        Assert.Equal(2, pending[0].SendCount);
    }
}