using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IChatHubService _chatHubService;

    public MessageService(
        IMessageRepository messageRepository,
        IGroupRepository groupRepository,
        IChatHubService chatHubService)
    {
        _messageRepository = messageRepository;
        _groupRepository = groupRepository;
        _chatHubService = chatHubService;
    }

    public async Task<MessageResponse> SendMessageAsync(Guid groupId, SendMessageRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        bool isMember = await _groupRepository.IsMemberAsync(groupId, currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new AuthorizationException("You are not a member of this group.");
        }

        DateTime now = DateTime.UtcNow;

        Message message = new Message
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            SenderId = currentUserId,
            SenderType = SenderType.User,
            Content = request.Content,
            AiVisible = group.AiMonitoringEnabled,
            CreatedAt = now
        };

        await _messageRepository.CreateAsync(message, cancellationToken);

        // Fetch the message with sender info
        Message? createdMessage = await _messageRepository.GetByIdAsync(message.Id, cancellationToken);

        MessageResponse response = MapToResponse(createdMessage!);

        // Broadcast to group members via SignalR
        await _chatHubService.BroadcastMessageAsync(groupId, response, cancellationToken);

        return response;
    }

    public async Task<PaginatedResponse<MessageResponse>> GetMessagesAsync(Guid groupId, string currentUserId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        bool isMember = await _groupRepository.IsMemberAsync(groupId, currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new AuthorizationException("You are not a member of this group.");
        }

        // Clamp page and pageSize to reasonable values
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        List<Message> messages = await _messageRepository.GetByGroupIdAsync(groupId, page, pageSize, cancellationToken);
        int totalCount = await _messageRepository.GetCountByGroupIdAsync(groupId, cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedResponse<MessageResponse>
        {
            Items = messages.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    private static MessageResponse MapToResponse(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            GroupId = message.GroupId,
            SenderId = message.SenderId,
            SenderUserName = message.Sender?.UserName,
            SenderDisplayName = message.Sender?.DisplayName,
            SenderType = message.SenderType.ToString(),
            Content = message.Content,
            AttachmentUrl = message.AttachmentUrl,
            AttachmentType = message.AttachmentType,
            AttachmentName = message.AttachmentName,
            CreatedAt = message.CreatedAt
        };
    }
}
