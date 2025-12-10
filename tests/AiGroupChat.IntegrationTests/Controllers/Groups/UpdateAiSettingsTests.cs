using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class UpdateAiSettingsTests : IntegrationTestBase
{
    public UpdateAiSettingsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateAiSettings_AsOwner_ReturnsUpdatedGroup()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Test Group");

        // AI monitoring should be false by default
        Assert.False(createdGroup.AiMonitoringEnabled);

        // Act - enable AI monitoring
        HttpResponseMessage response = await Groups.UpdateAiSettingsRawAsync(
            createdGroup.Id,
            aiMonitoringEnabled: true);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GroupResponse? group = await response.Content.ReadFromJsonAsync<GroupResponse>();
        Assert.NotNull(group);
        Assert.True(group.AiMonitoringEnabled);
    }

    [Fact]
    public async Task UpdateAiSettings_AsNonMember_Returns403()
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
        HttpResponseMessage response = await Groups.UpdateAiSettingsRawAsync(
            createdGroup.Id,
            aiMonitoringEnabled: true);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAiSettings_WithInvalidProviderId_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();
        GroupResponse createdGroup = await Groups.CreateGroupAsync("Test Group");

        Guid invalidProviderId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.UpdateAiSettingsRawAsync(
            createdGroup.Id,
            aiProviderId: invalidProviderId);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAiSettings_WithNonExistentGroupId_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.UpdateAiSettingsRawAsync(
            nonExistentId,
            aiMonitoringEnabled: true);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAiSettings_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        Guid someGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Groups.UpdateAiSettingsRawAsync(
            someGroupId,
            aiMonitoringEnabled: true);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}