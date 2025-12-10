using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class UpdateGroupTests : IntegrationTestBase
{
    public UpdateGroupTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Update_AsOwner_ReturnsUpdatedGroup()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Original Name");

        // Act
        HttpResponseMessage response = await Groups.UpdateGroupRawAsync(createdGroup.Id, "Updated Name");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupResponse? group = await response.Content.ReadFromJsonAsync<GroupResponse>();
        Assert.NotNull(group);
        Assert.Equal("Updated Name", group.Name);
        Assert.Equal(createdGroup.Id, group.Id);
    }

    [Fact]
    public async Task Update_AsNonMember_Returns403()
    {
        // Arrange - create a group with user1
        await Auth.CreateAuthenticatedUserAsync(
            email: "user1@example.com",
            userName: "user1");

        GroupResponse createdGroup = await Groups.CreateGroupAsync("User1 Group");

        // Switch to user2 who is not a member
        Auth.ClearAuthToken();
        await Auth.CreateAuthenticatedUserAsync(
            email: "user2@example.com",
            userName: "user2");

        // Act
        HttpResponseMessage response = await Groups.UpdateGroupRawAsync(createdGroup.Id, "Hacked Name");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task Update_WithNonExistentId_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.UpdateGroupRawAsync(nonExistentId, "Some Name");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithEmptyName_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Original Name");

        // Act
        HttpResponseMessage response = await Groups.UpdateGroupRawAsync(createdGroup.Id, "");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        Guid someGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.UpdateGroupRawAsync(someGroupId, "Some Name");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}