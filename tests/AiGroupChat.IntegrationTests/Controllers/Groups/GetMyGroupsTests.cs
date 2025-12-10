using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class GetMyGroupsTests : IntegrationTestBase
{
    public GetMyGroupsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMyGroups_WithNoGroups_ReturnsEmptyList()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await Groups.GetMyGroupsRawAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<GroupResponse>? groups = await response.Content.ReadFromJsonAsync<List<GroupResponse>>();
        Assert.NotNull(groups);
        Assert.Empty(groups);
    }

    [Fact]
    public async Task GetMyGroups_ReturnsOnlyUserGroups()
    {
        // Arrange - create first user and their groups
        await Auth.CreateAuthenticatedUserAsync(
            email: "user1@example.com",
            userName: "user1");

        await Groups.CreateGroupAsync("User1 Group A");
        await Groups.CreateGroupAsync("User1 Group B");

        // Create second user and their group
        Auth.ClearAuthToken();
        await Auth.CreateAuthenticatedUserAsync(
            email: "user2@example.com",
            userName: "user2");

        await Groups.CreateGroupAsync("User2 Group");

        // Act - get groups for user2
        HttpResponseMessage response = await Groups.GetMyGroupsRawAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<GroupResponse>? groups = await response.Content.ReadFromJsonAsync<List<GroupResponse>>();
        Assert.NotNull(groups);
        Assert.Single(groups);
        Assert.Equal("User2 Group", groups[0].Name);
    }

    [Fact]
    public async Task GetMyGroups_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Groups.GetMyGroupsRawAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}