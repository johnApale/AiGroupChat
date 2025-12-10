using System.Security.Claims;
using AiGroupChat.API.Hubs;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Base class for ChatHub unit tests.
/// Provides mocked dependencies and helper methods for testing hub methods.
/// </summary>
public abstract class ChatHubTestBase
{
    // Repository mocks
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<IGroupMemberRepository> GroupMemberRepositoryMock;

    // Service mocks
    protected readonly Mock<IConnectionTracker> ConnectionTrackerMock;
    protected readonly Mock<IChatHubService> ChatHubServiceMock;
    protected readonly Mock<ILogger<API.Hubs.ChatHub>> LoggerMock;

    // SignalR infrastructure mocks
    protected readonly Mock<IHubCallerClients> ClientsMock;
    protected readonly Mock<IGroupManager> GroupsMock;
    protected readonly Mock<HubCallerContext> ContextMock;
    protected readonly Mock<IClientProxy> AllClientProxyMock;
    protected readonly Mock<IClientProxy> GroupClientProxyMock;
    protected readonly Mock<IClientProxy> OthersInGroupClientProxyMock;

    // The hub under test
    protected readonly API.Hubs.ChatHub Hub;

    // Test data
    protected readonly string TestUserId = "test-user-id-123";
    protected readonly string TestUserName = "testuser";
    protected readonly string TestDisplayName = "Test User";
    protected readonly string TestConnectionId = "test-connection-id";
    protected readonly Guid TestGroupId = Guid.NewGuid();

    protected readonly User TestUser;

    protected ChatHubTestBase()
    {
        // Initialize repository mocks
        GroupRepositoryMock = new Mock<IGroupRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        GroupMemberRepositoryMock = new Mock<IGroupMemberRepository>();

        // Initialize service mocks
        ConnectionTrackerMock = new Mock<IConnectionTracker>();
        ChatHubServiceMock = new Mock<IChatHubService>();
        LoggerMock = new Mock<ILogger<API.Hubs.ChatHub>>();

        // Initialize SignalR infrastructure mocks
        ClientsMock = new Mock<IHubCallerClients>();
        GroupsMock = new Mock<IGroupManager>();
        ContextMock = new Mock<HubCallerContext>();
        AllClientProxyMock = new Mock<IClientProxy>();
        GroupClientProxyMock = new Mock<IClientProxy>();
        OthersInGroupClientProxyMock = new Mock<IClientProxy>();

        // Setup test user
        TestUser = new User
        {
            Id = TestUserId,
            UserName = TestUserName,
            DisplayName = TestDisplayName,
            Email = "test@example.com"
        };

        // Setup context to return test user ID and connection ID
        ContextMock
            .Setup(c => c.User)
            .Returns(CreateClaimsPrincipal(TestUserId));

        ContextMock
            .Setup(c => c.ConnectionId)
            .Returns(TestConnectionId);

        // Setup clients mock
        ClientsMock
            .Setup(c => c.All)
            .Returns(AllClientProxyMock.Object);

        ClientsMock
            .Setup(c => c.Group(It.IsAny<string>()))
            .Returns(GroupClientProxyMock.Object);

        ClientsMock
            .Setup(c => c.OthersInGroup(It.IsAny<string>()))
            .Returns(OthersInGroupClientProxyMock.Object);

        // Create the hub
        Hub = new API.Hubs.ChatHub(
            GroupRepositoryMock.Object,
            UserRepositoryMock.Object,
            GroupMemberRepositoryMock.Object,
            ConnectionTrackerMock.Object,
            ChatHubServiceMock.Object,
            LoggerMock.Object);

        // Inject the mocked SignalR context into the hub
        // Hub.Context, Hub.Clients, and Hub.Groups are set via reflection or property injection
        SetHubContext();
    }

    /// <summary>
    /// Sets up the hub's Context, Clients, and Groups properties with mocks.
    /// SignalR hubs don't have public setters, so we use reflection.
    /// </summary>
    private void SetHubContext()
    {
        // Use the Hub's assignable properties through the base Hub class
        Type hubType = typeof(Hub);

        // Set Context
        System.Reflection.PropertyInfo? contextProperty = hubType.GetProperty("Context");
        contextProperty?.SetValue(Hub, ContextMock.Object);

        // Set Clients
        System.Reflection.PropertyInfo? clientsProperty = hubType.GetProperty("Clients");
        clientsProperty?.SetValue(Hub, ClientsMock.Object);

        // Set Groups
        System.Reflection.PropertyInfo? groupsProperty = hubType.GetProperty("Groups");
        groupsProperty?.SetValue(Hub, GroupsMock.Object);
    }

    /// <summary>
    /// Creates a ClaimsPrincipal with the specified user ID
    /// </summary>
    protected static ClaimsPrincipal CreateClaimsPrincipal(string userId)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Sets up the GroupRepository to return that the user is a member of the test group
    /// </summary>
    protected void SetupUserIsMember(bool isMember = true)
    {
        GroupRepositoryMock
            .Setup(r => r.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isMember);
    }

    /// <summary>
    /// Sets up the UserRepository to return the test user
    /// </summary>
    protected void SetupUserExists()
    {
        UserRepositoryMock
            .Setup(r => r.FindByIdAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestUser);
    }

    /// <summary>
    /// Sets up the GroupMemberRepository to return shared users
    /// </summary>
    protected void SetupSharedUsers(List<string> sharedUserIds)
    {
        GroupMemberRepositoryMock
            .Setup(r => r.GetUsersWhoShareGroupsWithAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sharedUserIds);
    }

    /// <summary>
    /// Sets up the ConnectionTracker for connection scenarios
    /// </summary>
    protected void SetupConnectionTracker(bool isFirstConnection = true, bool isLastConnection = true)
    {
        ConnectionTrackerMock
            .Setup(t => t.AddConnection(TestUserId, TestConnectionId))
            .Returns(isFirstConnection);

        ConnectionTrackerMock
            .Setup(t => t.RemoveConnection(TestUserId, TestConnectionId))
            .Returns(isLastConnection);
    }

    /// <summary>
    /// Gets the SignalR group name for a group ID
    /// </summary>
    protected static string GetGroupName(Guid groupId)
    {
        return $"group-{groupId}";
    }

    /// <summary>
    /// Gets the personal channel name for a user ID
    /// </summary>
    protected static string GetPersonalChannelName(string userId)
    {
        return $"user-{userId}";
    }
}