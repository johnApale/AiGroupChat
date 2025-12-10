using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Groups;

public class CreateGroupTests : IntegrationTestBase
{
    public CreateGroupTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_WithValidRequest_Returns201AndGroup()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await Groups.CreateGroupRawAsync("My Test Group");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        GroupResponse? group = await response.Content.ReadFromJsonAsync<GroupResponse>();
        Assert.NotNull(group);
        Assert.Equal("My Test Group", group.Name);
        Assert.NotEqual(Guid.Empty, group.Id);
        Assert.False(group.AiMonitoringEnabled);
        Assert.NotEqual(Guid.Empty, group.AiProviderId);
        Assert.NotNull(group.AiProvider);
        Assert.Single(group.Members);
        Assert.Equal("Owner", group.Members[0].Role);
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        // Act
        HttpResponseMessage response = await Groups.CreateGroupRawAsync("");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTooLongName_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        string longName = new string('a', 201); // Exceeds 200 character limit

        // Act
        HttpResponseMessage response = await Groups.CreateGroupRawAsync(longName);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutToken_Returns401()
    {
        // Arrange - ensure no auth token is set
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Groups.CreateGroupRawAsync("Test Group");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}