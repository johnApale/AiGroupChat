using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiGroupChat.Infrastructure.Repositories;

public class GroupInvitationRepository : IGroupInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public GroupInvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GroupInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GroupInvitations
            .Include(i => i.Group)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<GroupInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.GroupInvitations
            .Include(i => i.Group)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<GroupInvitation?> GetPendingByEmailAndGroupAsync(string email, Guid groupId, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.ToLowerInvariant();
        return await _context.GroupInvitations
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => 
                i.GroupId == groupId && 
                i.Email == normalizedEmail && 
                i.Status == InvitationStatus.Pending, 
                cancellationToken);
    }

    public async Task<List<GroupInvitation>> GetPendingByGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupInvitations
            .Include(i => i.InvitedBy)
            .Where(i => i.GroupId == groupId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<GroupInvitation> CreateAsync(GroupInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.GroupInvitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);
        return invitation;
    }

    public async Task UpdateAsync(GroupInvitation invitation, CancellationToken cancellationToken = default)
    {
        _context.GroupInvitations.Update(invitation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}