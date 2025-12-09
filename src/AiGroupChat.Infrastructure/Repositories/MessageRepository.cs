using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiGroupChat.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<List<Message>> GetByGroupIdAsync(Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        int skip = (page - 1) * pageSize;

        List<Message> messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.GroupId == groupId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task<int> GetCountByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        int count = await _context.Messages
            .Where(m => m.GroupId == groupId)
            .CountAsync(cancellationToken);

        return count;
    }

    public async Task<Message?> GetByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        Message? message = await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        return message;
    }
}
