using AiGroupChat.API.Hubs;
using AiGroupChat.API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Base class for ChatHubService unit tests.
/// Provides mocked IHubContext and helper methods for verifying broadcasts.
/// </summary>
public abstract class ChatHubServiceTestBase
{
    // Hub context mocks
    protected readonly Mock<IHubContext<ChatHub>> HubContextMock;
    protected readonly Mock<IHubClients> HubClientsMock;
    protected readonly Mock<IClientProxy> GroupClientProxyMock;
    protected readonly Mock<IClientProxy> GroupsClientProxyMock;
    protected readonly Mock<ILogger<API.Services.ChatHubService>> LoggerMock;

    // The service under test
    protected readonly API.Services.ChatHubService ChatHubService;

    // Test data
    protected readonly Guid TestGroupId = Guid.NewGuid();
    protected readonly string TestUserId = "test-user-id-123";

    // Capture sent messages for verification
    protected string? CapturedMethodName;
    protected object[]? CapturedArgs;
    protected string? CapturedGroupName;
    protected IReadOnlyList<string>? CapturedGroupNames;

    protected ChatHubServiceTestBase()
    {
        // Initialize mocks
        HubContextMock = new Mock<IHubContext<ChatHub>>();
        HubClientsMock = new Mock<IHubClients>();
        GroupClientProxyMock = new Mock<IClientProxy>();
        GroupsClientProxyMock = new Mock<IClientProxy>();
        LoggerMock = new Mock<ILogger<API.Services.ChatHubService>>();

        // Setup HubContext to return HubClients
        HubContextMock
            .Setup(c => c.Clients)
            .Returns(HubClientsMock.Object);

        // Setup single group client proxy with capture
        HubClientsMock
            .Setup(c => c.Group(It.IsAny<string>()))
            .Callback<string>(groupName => CapturedGroupName = groupName)
            .Returns(GroupClientProxyMock.Object);

        // Setup multiple groups client proxy with capture
        HubClientsMock
            .Setup(c => c.Groups(It.IsAny<IReadOnlyList<string>>()))
            .Callback<IReadOnlyList<string>>(groupNames => CapturedGroupNames = groupNames)
            .Returns(GroupsClientProxyMock.Object);

        // Setup client proxy to capture method and args
        GroupClientProxyMock
            .Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                CapturedMethodName = method;
                CapturedArgs = args;
            })
            .Returns(Task.CompletedTask);

        GroupsClientProxyMock
            .Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((method, args, ct) =>
            {
                CapturedMethodName = method;
                CapturedArgs = args;
            })
            .Returns(Task.CompletedTask);

        // Create the service
        ChatHubService = new API.Services.ChatHubService(
            HubContextMock.Object,
            LoggerMock.Object);
    }

    /// <summary>
    /// Resets captured data between tests
    /// </summary>
    protected void ResetCaptures()
    {
        CapturedMethodName = null;
        CapturedArgs = null;
        CapturedGroupName = null;
        CapturedGroupNames = null;
    }

    /// <summary>
    /// Gets the expected SignalR group name for a group ID
    /// </summary>
    protected static string GetGroupName(Guid groupId)
    {
        return $"group-{groupId}";
    }

    /// <summary>
    /// Gets the expected personal channel name for a user ID
    /// </summary>
    protected static string GetPersonalChannelName(string userId)
    {
        return $"user-{userId}";
    }
}