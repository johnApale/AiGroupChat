using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupInvitations;

public class GetPendingInvitationsTests : IntegrationTestBase
{
    public GetPendingInvitationsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPendingInvitations_AsOwner_ReturnsInvitations()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send some invitations
        await Invitations.InviteMembersAsync(group.Id, new List<string>
        {
            "user1@example.com",
            "user2@example.com"
        });

        // Act
        HttpResponseMessage response = await Invitations.GetPendingInvitationsRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<InvitationResponse>? result = await response.Content.ReadFromJsonAsync<List<InvitationResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Email == "user1@example.com");
        Assert.Contains(result, i => i.Email == "user2@example.com");
        Assert.All(result, i => Assert.Equal("Pending", i.Status));
    }

    [Fact]
    public async Task GetPendingInvitations_AsAdmin_ReturnsInvitations()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send invitation as owner
        await Invitations.InviteMemberAsync(group.Id, "invited@example.com");

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
        HttpResponseMessage response = await Invitations.GetPendingInvitationsRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<InvitationResponse>? result = await response.Content.ReadFromJsonAsync<List<InvitationResponse>>();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetPendingInvitations_AsMember_Returns403()
    {
        // Arrange
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
        HttpResponseMessage response = await Invitations.GetPendingInvitationsRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingInvitations_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();
        Guid groupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.GetPendingInvitationsRawAsync(groupId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingInvitations_WithNonexistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Invitations.GetPendingInvitationsRawAsync(nonExistentGroupId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingInvitations_WithNoPending_ReturnsEmptyList()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act
        List<InvitationResponse> result = await Invitations.GetPendingInvitationsAsync(group.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsCorrectInviterInfo()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "The Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");
        await Invitations.InviteMemberAsync(group.Id, "invited@example.com");

        // Act
        List<InvitationResponse> result = await Invitations.GetPendingInvitationsAsync(group.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("The Owner", result[0].InvitedByUserName);
    }
}