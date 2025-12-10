using System.Net;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class DeleteGroupTests : IntegrationTestBase
{
    public DeleteGroupTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Delete_AsOwner_Returns204()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Group To Delete");

        // Act
        HttpResponseMessage response = await Groups.DeleteGroupRawAsync(createdGroup.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify group is actually deleted
        HttpResponseMessage getResponse = await Groups.GetGroupRawAsync(createdGroup.Id);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_AsNonMember_Returns403()
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
        HttpResponseMessage response = await Groups.DeleteGroupRawAsync(createdGroup.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.DeleteGroupRawAsync(nonExistentId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        Guid someGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.DeleteGroupRawAsync(someGroupId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}