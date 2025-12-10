using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupMembers;

public class UpdateMemberRoleTests : IntegrationTestBase
{
    public UpdateMemberRoleTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateMemberRole_AsOwner_ToAdmin_ReturnsUpdatedMember()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add a member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Act - Promote to Admin
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, member.User.Id, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupMemberResponse? updatedMember = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(updatedMember);
        Assert.Equal(member.User.Id, updatedMember.UserId);
        Assert.Equal("Admin", updatedMember.Role);
    }

    [Fact]
    public async Task UpdateMemberRole_AsOwner_ToMember_ReturnsUpdatedMember()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add a member and promote to admin
        Auth.ClearAuthToken();
        AuthResponse admin = await Auth.CreateAuthenticatedUserAsync(
            email: "admin@example.com",
            userName: "admin",
            displayName: "Admin User");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin.User.Id, "Admin");

        // Act - Demote to Member
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, admin.User.Id, "Member");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupMemberResponse? updatedMember = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(updatedMember);
        Assert.Equal("Member", updatedMember.Role);
    }

    [Fact]
    public async Task UpdateMemberRole_AsAdmin_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add an admin
        Auth.ClearAuthToken();
        AuthResponse admin = await Auth.CreateAuthenticatedUserAsync(
            email: "admin@example.com",
            userName: "admin",
            displayName: "Admin User");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin.User.Id, "Admin");

        // Add a regular member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Switch to admin
        Auth.SetAuthToken(admin.AccessToken);

        // Act - Admin tries to change member's role
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, member.User.Id, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_AsMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add two members
        Auth.ClearAuthToken();
        AuthResponse member1 = await Auth.CreateAuthenticatedUserAsync(
            email: "member1@example.com",
            userName: "member1",
            displayName: "Member One");

        Auth.ClearAuthToken();
        AuthResponse member2 = await Auth.CreateAuthenticatedUserAsync(
            email: "member2@example.com",
            userName: "member2",
            displayName: "Member Two");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member1.User.Id);
        await Members.AddMemberAsync(group.Id, member2.User.Id);

        // Switch to regular member
        Auth.SetAuthToken(member1.AccessToken);

        // Act
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, member2.User.Id, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_ChangeOwnerRole_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act - Try to change owner's role
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, owner.User.Id, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_ToOwner_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add a member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Act - Try to set role to Owner
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, member.User.Id, "Owner");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithInvalidRole_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add a member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Act - Try to set invalid role
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, member.User.Id, "SuperAdmin");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithNonExistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(nonExistentGroupId, userId, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithNonExistentMember_Returns404()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        string nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(group.Id, nonExistentUserId, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberRole_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.UpdateMemberRoleRawAsync(groupId, userId, "Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}