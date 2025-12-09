namespace AiGroupChat.Application.Interfaces;

/// <summary>
/// Repository for group member queries, primarily for presence and notification purposes.
/// </summary>
public interface IGroupMemberRepository
{
    /// <summary>
    /// Gets all user IDs who share at least one group with the specified user.
    /// Used for presence broadcasts (UserOnline/UserOffline events).
    /// Excludes the user themselves from the result.
    /// </summary>
    Task<List<string>> GetUsersWhoShareGroupsWithAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user IDs who are members of the specified group.
    /// Used for broadcasting personal channel notifications.
    /// </summary>
    Task<List<string>> GetGroupMemberIdsAsync(Guid groupId, CancellationToken cancellationToken = default);
}
