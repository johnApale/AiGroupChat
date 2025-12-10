using System.Net;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupMembers;

public class RemoveMemberTests : IntegrationTestBase
{
    public RemoveMemberTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RemoveMember_AsOwner_RemoveMember_Returns204()
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

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify member is removed
        List<GroupMemberResponse> members = await Members.GetMembersAsync(group.Id);
        Assert.Single(members); // Only owner remains
    }

    [Fact]
    public async Task RemoveMember_AsOwner_RemoveAdmin_Returns204()
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

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, admin.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_AsAdmin_RemoveMember_Returns204()
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

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_AsAdmin_RemoveOtherAdmin_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add two admins
        Auth.ClearAuthToken();
        AuthResponse admin1 = await Auth.CreateAuthenticatedUserAsync(
            email: "admin1@example.com",
            userName: "admin1",
            displayName: "Admin One");

        Auth.ClearAuthToken();
        AuthResponse admin2 = await Auth.CreateAuthenticatedUserAsync(
            email: "admin2@example.com",
            userName: "admin2",
            displayName: "Admin Two");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin1.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin1.User.Id, "Admin");
        await Members.AddMemberAsync(group.Id, admin2.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin2.User.Id, "Admin");

        // Switch to admin1
        Auth.SetAuthToken(admin1.AccessToken);

        // Act - Admin tries to remove another admin
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, admin2.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_AsMember_Returns403()
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
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, member2.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_RemoveOwner_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act - Try to remove owner
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, owner.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_AsNonMember_Returns403()
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

        // Create non-member
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_WithNonExistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(nonExistentGroupId, userId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_WithNonExistentMember_Returns404()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        string nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(group.Id, nonExistentUserId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.RemoveMemberRawAsync(groupId, userId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}