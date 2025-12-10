using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupMembers;

public class AddMemberTests : IntegrationTestBase
{
    public AddMemberTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AddMember_AsOwner_Returns201AndMember()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create another user to add
        Auth.ClearAuthToken();
        AuthResponse newUser = await Auth.CreateAuthenticatedUserAsync(
            email: "newuser@example.com",
            userName: "newuser",
            displayName: "New User");

        // Switch back to owner
        Auth.SetAuthToken(owner.AccessToken);

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, newUser.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        GroupMemberResponse? member = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(member);
        Assert.Equal(newUser.User.Id, member.UserId);
        Assert.Equal("newuser", member.UserName);
        Assert.Equal("New User", member.DisplayName);
        Assert.Equal("Member", member.Role);
    }

    [Fact]
    public async Task AddMember_AsAdmin_Returns201AndMember()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create admin user and add to group
        Auth.ClearAuthToken();
        AuthResponse admin = await Auth.CreateAuthenticatedUserAsync(
            email: "admin@example.com",
            userName: "admin",
            displayName: "Admin User");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, admin.User.Id);
        await Members.UpdateMemberRoleAsync(group.Id, admin.User.Id, "Admin");

        // Create a third user to add
        Auth.ClearAuthToken();
        AuthResponse newUser = await Auth.CreateAuthenticatedUserAsync(
            email: "newuser@example.com",
            userName: "newuser",
            displayName: "New User");

        // Switch to admin
        Auth.SetAuthToken(admin.AccessToken);

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, newUser.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        GroupMemberResponse? member = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(member);
        Assert.Equal(newUser.User.Id, member.UserId);
        Assert.Equal("Member", member.Role);
    }

    [Fact]
    public async Task AddMember_AsMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create regular member and add to group
        Auth.ClearAuthToken();
        AuthResponse regularMember = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, regularMember.User.Id);

        // Create a third user to try to add
        Auth.ClearAuthToken();
        AuthResponse newUser = await Auth.CreateAuthenticatedUserAsync(
            email: "newuser@example.com",
            userName: "newuser",
            displayName: "New User");

        // Switch to regular member
        Auth.SetAuthToken(regularMember.AccessToken);

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, newUser.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_AsNonMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create two other users (non-member and user to add)
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        Auth.ClearAuthToken();
        AuthResponse newUser = await Auth.CreateAuthenticatedUserAsync(
            email: "newuser@example.com",
            userName: "newuser",
            displayName: "New User");

        // Switch to non-member
        Auth.SetAuthToken(nonMember.AccessToken);

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, newUser.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WithNonExistentGroup_Returns404()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(nonExistentGroupId, user.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WithNonExistentUser_Returns404()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        string nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, nonExistentUserId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WithExistingMember_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create another user and add them
        Auth.ClearAuthToken();
        AuthResponse existingMember = await Auth.CreateAuthenticatedUserAsync(
            email: "existing@example.com",
            userName: "existing",
            displayName: "Existing Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, existingMember.User.Id);

        // Act - Try to add them again
        HttpResponseMessage response = await Members.AddMemberRawAsync(group.Id, existingMember.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.AddMemberRawAsync(groupId, userId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}