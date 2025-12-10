using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;
using MessageResponse = AiGroupChat.Application.DTOs.Messages.MessageResponse;

namespace AiGroupChat.IntegrationTests.Controllers.Messages;

public class GetMessagesTests : IntegrationTestBase
{
    public GetMessagesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMessages_AsMember_ReturnsMessages()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send some messages
        await Messages.SendMessageAsync(group.Id, "First message");
        await Messages.SendMessageAsync(group.Id, "Second message");
        await Messages.SendMessageAsync(group.Id, "Third message");

        // Act
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedResponse<MessageResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetMessages_AsRegularMember_ReturnsMessages()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send a message as owner
        await Messages.SendMessageAsync(group.Id, "Owner message");

        // Add a member
        Auth.ClearAuthToken();
        AuthResponse member = await Auth.CreateAuthenticatedUserAsync(
            email: "member@example.com",
            userName: "member",
            displayName: "Regular Member");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, member.User.Id);

        // Switch to member
        Auth.SetAuthToken(member.AccessToken);

        // Act
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedResponse<MessageResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Owner message", result.Items[0].Content);
    }

    [Fact]
    public async Task GetMessages_WithPagination_ReturnsCorrectPage()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send 5 messages
        for (int i = 1; i <= 5; i++)
        {
            await Messages.SendMessageAsync(group.Id, $"Message {i}");
        }

        // Act - Get page 1 with pageSize 2
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id, page: 1, pageSize: 2);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedResponse<MessageResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetMessages_WithPagination_Page2_ReturnsCorrectPage()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Send 5 messages
        for (int i = 1; i <= 5; i++)
        {
            await Messages.SendMessageAsync(group.Id, $"Message {i}");
        }

        // Act - Get page 2 with pageSize 2
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id, page: 2, pageSize: 2);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedResponse<MessageResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetMessages_EmptyGroup_ReturnsEmptyList()
    {
        // Arrange - Create owner and group
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act - Don't send any messages
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PaginatedResponse<MessageResponse>? result = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetMessages_AsNonMember_Returns403()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create non-member
        Auth.ClearAuthToken();
        AuthResponse nonMember = await Auth.CreateAuthenticatedUserAsync(
            email: "nonmember@example.com",
            userName: "nonmember",
            displayName: "Non Member");

        // Act
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(group.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_WithNonExistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(nonExistentGroupId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Messages.GetMessagesRawAsync(groupId);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}