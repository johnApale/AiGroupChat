using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupOwner;

public class TransferOwnershipTests : IntegrationTestBase
{
    public TransferOwnershipTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task TransferOwnership_AsOwner_ToMember_Returns200()
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
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupMemberResponse? newOwner = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(newOwner);
        Assert.Equal(member.User.Id, newOwner.UserId);
        Assert.Equal("Owner", newOwner.Role);

        // Verify old owner is now Admin
        List<GroupMemberResponse> members = await Members.GetMembersAsync(group.Id);
        GroupMemberResponse? oldOwner = members.FirstOrDefault(m => m.UserId == owner.User.Id);
        Assert.NotNull(oldOwner);
        Assert.Equal("Admin", oldOwner.Role);
    }

    [Fact]
    public async Task TransferOwnership_AsOwner_ToAdmin_Returns200()
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
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, admin.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupMemberResponse? newOwner = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        Assert.NotNull(newOwner);
        Assert.Equal(admin.User.Id, newOwner.UserId);
        Assert.Equal("Owner", newOwner.Role);
    }

    [Fact]
    public async Task TransferOwnership_AsAdmin_Returns403()
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

        // Add a member to transfer to
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
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_AsMember_Returns403()
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
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, member2.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_AsNonMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add a member to the group
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Create a non-member
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        // Act - Non-member tries to transfer ownership
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, member.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_ToSelf_Returns400()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act - Try to transfer to self
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, owner.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_ToNonMember_Returns404()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create a user who is not a member
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        Auth.SetAuthToken(owner.AccessToken);

        // Act - Try to transfer to non-member
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(group.Id, nonMember.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_WithNonExistentGroup_Returns404()
    {
        // Arrange
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(nonExistentGroupId, user.User.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TransferOwnership_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();
        string userId = Guid.NewGuid().ToString();

        // Act
        HttpResponseMessage response = await Members.TransferOwnershipRawAsync(groupId, userId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}