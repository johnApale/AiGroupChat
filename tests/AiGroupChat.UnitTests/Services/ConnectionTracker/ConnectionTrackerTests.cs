using AiGroupChat.API.Services;

namespace AiGroupChat.UnitTests.Services.ConnectionTracker;

public class ConnectionTrackerTests
{
    private readonly API.Services.ConnectionTracker _connectionTracker;

    public ConnectionTrackerTests()
    {
        _connectionTracker = new API.Services.ConnectionTracker();
    }

    #region AddConnection Tests

    [Fact]
    public void AddConnection_FirstConnection_ReturnsTrue()
    {
        // Arrange
        string userId = "user-123";
        string connectionId = "conn-1";

        // Act
        bool result = _connectionTracker.AddConnection(userId, connectionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AddConnection_SecondConnection_ReturnsFalse()
    {
        // Arrange
        string userId = "user-123";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        _connectionTracker.AddConnection(userId, connectionId1);

        // Act
        bool result = _connectionTracker.AddConnection(userId, connectionId2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AddConnection_DifferentUsers_BothReturnTrue()
    {
        // Arrange
        string userId1 = "user-123";
        string userId2 = "user-456";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        // Act
        bool result1 = _connectionTracker.AddConnection(userId1, connectionId1);
        bool result2 = _connectionTracker.AddConnection(userId2, connectionId2);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    #endregion

    #region RemoveConnection Tests

    [Fact]
    public void RemoveConnection_LastConnection_ReturnsTrue()
    {
        // Arrange
        string userId = "user-123";
        string connectionId = "conn-1";

        _connectionTracker.AddConnection(userId, connectionId);

        // Act
        bool result = _connectionTracker.RemoveConnection(userId, connectionId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RemoveConnection_NotLastConnection_ReturnsFalse()
    {
        // Arrange
        string userId = "user-123";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        _connectionTracker.AddConnection(userId, connectionId1);
        _connectionTracker.AddConnection(userId, connectionId2);

        // Act
        bool result = _connectionTracker.RemoveConnection(userId, connectionId1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveConnection_UserNotTracked_ReturnsFalse()
    {
        // Arrange
        string userId = "user-123";
        string connectionId = "conn-1";

        // Act
        bool result = _connectionTracker.RemoveConnection(userId, connectionId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveConnection_AfterAllConnectionsRemoved_UserGoesOffline()
    {
        // Arrange
        string userId = "user-123";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        _connectionTracker.AddConnection(userId, connectionId1);
        _connectionTracker.AddConnection(userId, connectionId2);

        // Act
        _connectionTracker.RemoveConnection(userId, connectionId1);
        bool wasLastConnection = _connectionTracker.RemoveConnection(userId, connectionId2);

        // Assert
        Assert.True(wasLastConnection);
        Assert.False(_connectionTracker.IsOnline(userId));
    }

    #endregion

    #region GetConnections Tests

    [Fact]
    public void GetConnections_UserWithConnections_ReturnsAllConnectionIds()
    {
        // Arrange
        string userId = "user-123";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        _connectionTracker.AddConnection(userId, connectionId1);
        _connectionTracker.AddConnection(userId, connectionId2);

        // Act
        IReadOnlyList<string> connections = _connectionTracker.GetConnections(userId);

        // Assert
        Assert.Equal(2, connections.Count);
        Assert.Contains(connectionId1, connections);
        Assert.Contains(connectionId2, connections);
    }

    [Fact]
    public void GetConnections_UserNotTracked_ReturnsEmptyList()
    {
        // Arrange
        string userId = "user-123";

        // Act
        IReadOnlyList<string> connections = _connectionTracker.GetConnections(userId);

        // Assert
        Assert.Empty(connections);
    }

    [Fact]
    public void GetConnections_AfterRemovingConnection_ReturnsRemainingConnections()
    {
        // Arrange
        string userId = "user-123";
        string connectionId1 = "conn-1";
        string connectionId2 = "conn-2";

        _connectionTracker.AddConnection(userId, connectionId1);
        _connectionTracker.AddConnection(userId, connectionId2);
        _connectionTracker.RemoveConnection(userId, connectionId1);

        // Act
        IReadOnlyList<string> connections = _connectionTracker.GetConnections(userId);

        // Assert
        Assert.Single(connections);
        Assert.Contains(connectionId2, connections);
    }

    #endregion

    #region IsOnline Tests

    [Fact]
    public void IsOnline_UserWithConnection_ReturnsTrue()
    {
        // Arrange
        string userId = "user-123";
        string connectionId = "conn-1";

        _connectionTracker.AddConnection(userId, connectionId);

        // Act
        bool result = _connectionTracker.IsOnline(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOnline_UserWithoutConnection_ReturnsFalse()
    {
        // Arrange
        string userId = "user-123";

        // Act
        bool result = _connectionTracker.IsOnline(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOnline_AfterAllConnectionsRemoved_ReturnsFalse()
    {
        // Arrange
        string userId = "user-123";
        string connectionId = "conn-1";

        _connectionTracker.AddConnection(userId, connectionId);
        _connectionTracker.RemoveConnection(userId, connectionId);

        // Act
        bool result = _connectionTracker.IsOnline(userId);

        // Assert
        Assert.False(result);
    }

    #endregion
}
