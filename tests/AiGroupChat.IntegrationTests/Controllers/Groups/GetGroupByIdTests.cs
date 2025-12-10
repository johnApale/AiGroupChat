using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class GetGroupByIdTests : IntegrationTestBase
{
    public GetGroupByIdTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetById_AsMember_ReturnsGroup()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Test Group");

        // Act
        HttpResponseMessage response = await Groups.GetGroupRawAsync(createdGroup.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupResponse? group = await response.Content.ReadFromJsonAsync<GroupResponse>();
        Assert.NotNull(group);
        Assert.Equal(createdGroup.Id, group.Id);
        Assert.Equal("Test Group", group.Name);
    }

    [Fact]
    public async Task GetById_AsNonMember_Returns403()
    {
        // Arrange - create a group with user1
        await Auth.CreateAuthenticatedUserAsync(
            email: "user1@example.com",
            userName: "user1");

        GroupResponse createdGroup = await Groups.CreateGroupAsync("User1 Private Group");

        // Switch to user2 who is not a member
        Auth.ClearAuthToken();
        await Auth.CreateAuthenticatedUserAsync(
            email: "user2@example.com",
            userName: "user2");

        // Act
        HttpResponseMessage response = await Groups.GetGroupRawAsync(createdGroup.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.GetGroupRawAsync(nonExistentId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        Guid someGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.GetGroupRawAsync(someGroupId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}