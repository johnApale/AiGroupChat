namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Tracks SignalR connections per user for presence management.
/// Supports multiple connections per user (multiple tabs/devices).
/// </summary>
public interface IConnectionTracker
{
    /// <summary>
    /// Adds a connection for a user.
    /// </summary>
    /// <returns>True if this is the user's first connection (just came online).</returns>
    bool AddConnection(string userId, string connectionId);

    /// <summary>
    /// Removes a connection for a user.
    /// </summary>
    /// <returns>True if this was the user's last connection (just went offline).</returns>
    bool RemoveConnection(string userId, string connectionId);

    /// <summary>
    /// Gets all connection IDs for a user.
    /// </summary>
    IReadOnlyList<string> GetConnections(string userId);

    /// <summary>
    /// Checks if a user has any active connections.
    /// </summary>
    bool IsOnline(string userId);
}