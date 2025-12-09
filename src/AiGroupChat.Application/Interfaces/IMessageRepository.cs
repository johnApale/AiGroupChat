using AiGroupChat.Domain.Entities;

namespace AiGroupChat.Application.Interfaces;

public interface IMessageRepository
{
    /// <summary>
    /// Create a new message
    /// </summary>
    Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages for a group with pagination (newest first)
    /// </summary>
    Task<List<Message>> GetByGroupIdAsync(Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total message count for a group
    /// </summary>
    Task<int> GetCountByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a message by ID
    /// </summary>
    Task<Message?> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
}
