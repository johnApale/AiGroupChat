using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.GroupMembers;

public class GetMembersTests : IntegrationTestBase
{
    public GetMembersTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMembers_AsMember_ReturnsAllMembers()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Add two more members
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
        HttpResponseMessage response = await Members.GetMembersRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<GroupMemberResponse>? members = await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>();
        Assert.NotNull(members);
        Assert.Equal(3, members.Count);

        // Verify owner is in the list with Owner role
        GroupMemberResponse? ownerMember = members.FirstOrDefault(m => m.UserId == owner.User.Id);
        Assert.NotNull(ownerMember);
        Assert.Equal("Owner", ownerMember.Role);

        // Verify other members have Member role
        GroupMemberResponse? memberOne = members.FirstOrDefault(m => m.UserId == member1.User.Id);
        Assert.NotNull(memberOne);
        Assert.Equal("Member", memberOne.Role);
    }

    [Fact]
    public async Task GetMembers_AsNonMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create non-member user
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        // Act
        HttpResponseMessage response = await Members.GetMembersRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMembers_WithNonExistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Members.GetMembersRawAsync(nonExistentGroupId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMembers_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Members.GetMembersRawAsync(groupId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}