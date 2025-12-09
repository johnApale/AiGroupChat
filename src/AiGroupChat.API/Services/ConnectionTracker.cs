using System.Collections.Concurrent;
using AiGroupChat.Application.Interfaces;

namespace AiGroupChat.API.Services;

/// <summary>
/// In-memory connection tracker for SignalR presence management.
/// Supports multiple connections per user (multiple tabs/devices).
/// Thread-safe using ConcurrentDictionary.
/// </summary>
/// <remarks>
/// For MVP (single server). Future: Redis backplane for multi-server deployment.
/// </remarks>
public class ConnectionTracker : IConnectionTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public bool AddConnection(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userId, out HashSet<string>? connections))
            {
                connections.Add(connectionId);
                return false; // User already had connections, not newly online
            }

            _connections[userId] = new HashSet<string> { connectionId };
            return true; // First connection, user just came online
        }
    }

    /// <inheritdoc />
    public bool RemoveConnection(string userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out HashSet<string>? connections))
            {
                return false; // User not tracked
            }

            connections.Remove(connectionId);

            if (connections.Count == 0)
            {
                _connections.TryRemove(userId, out _);
                return true; // Last connection removed, user went offline
            }

            return false; // User still has other connections
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetConnections(string userId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userId, out HashSet<string>? connections))
            {
                return connections.ToList();
            }

            return Array.Empty<string>();
        }
    }

    /// <inheritdoc />
    public bool IsOnline(string userId)
    {
        return _connections.ContainsKey(userId);
    }
}
