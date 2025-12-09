using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Messages;

namespace AiGroupChat.Application.Interfaces;

public interface IMessageService
{
    /// <summary>
    /// Send a message to a group (must be a member)
    /// </summary>
    Task<MessageResponse> SendMessageAsync(Guid groupId, SendMessageRequest request, string currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages for a group with pagination (must be a member)
    /// </summary>
    Task<PaginatedResponse<MessageResponse>> GetMessagesAsync(Guid groupId, string currentUserId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
}
