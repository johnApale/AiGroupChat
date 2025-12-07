using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiGroupChat.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly ApplicationDbContext _context;

    public GroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Group> CreateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task<Group?> GetByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
    }

    public async Task<List<Group>> GetGroupsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .OrderByDescending(g => g.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Group> UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task DeleteAsync(Group group, CancellationToken cancellationToken = default)
    {
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsMemberAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsAdminAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && (m.Role == GroupRole.Admin || m.Role == GroupRole.Owner), cancellationToken);
    }

    public async Task<bool> IsOwnerAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Role == GroupRole.Owner, cancellationToken);
    }

    public async Task<GroupMember> AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default)
    {
        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        return member;
    }
}