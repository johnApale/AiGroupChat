using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.IntegrationTests.Infrastructure;
using MessageResponse = AiGroupChat.Application.DTOs.Messages.MessageResponse;

namespace AiGroupChat.IntegrationTests.Controllers.Messages;

public class SendMessageTests : IntegrationTestBase
{
    public SendMessageTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task SendMessage_AsMember_Returns201()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(group.Id, "Hello, world!");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        MessageResponse? message = await response.Content.ReadFromJsonAsync<MessageResponse>();
        Assert.NotNull(message);
        Assert.Equal("Hello, world!", message.Content);
        Assert.Equal(owner.User.Id, message.SenderId);
        Assert.Equal("owner", message.SenderUserName);
        Assert.Equal("Group Owner", message.SenderDisplayName);
        Assert.Equal("User", message.SenderType);
        Assert.Equal(group.Id, message.GroupId);
        Assert.NotEqual(Guid.Empty, message.Id);
    }

    [Fact]
    public async Task SendMessage_AsRegularMember_Returns201()
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

        // Switch to member
        Auth.SetAuthToken(member.AccessToken);

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(group.Id, "Hello from member!");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        MessageResponse? message = await response.Content.ReadFromJsonAsync<MessageResponse>();
        Assert.NotNull(message);
        Assert.Equal("Hello from member!", message.Content);
        Assert.Equal(member.User.Id, message.SenderId);
    }

    [Fact]
    public async Task SendMessage_AsNonMember_Returns403()
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
        HttpResponseMessage response = await Messages.SendMessageRawAsync(group.Id, "Hello!");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithEmptyContent_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(group.Id, "");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithTooLongContent_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        string longContent = new string('a', 10001); // Exceeds 10000 character limit

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(group.Id, longContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithNonExistentGroup_Returns404()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync();

        Guid nonExistentGroupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(nonExistentGroupId, "Hello!");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithoutToken_Returns401()
    {
        // Arrange
        Auth.ClearAuthToken();

        Guid groupId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await Messages.SendMessageRawAsync(groupId, "Hello!");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}