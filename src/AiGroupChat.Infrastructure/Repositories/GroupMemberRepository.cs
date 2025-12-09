using AiGroupChat.Application.Interfaces;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AiGroupChat.Infrastructure.Repositories;

public class GroupMemberRepository : IGroupMemberRepository
{
    private readonly ApplicationDbContext _context;

    public GroupMemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUsersWhoShareGroupsWithAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Get all group IDs the user is a member of
        List<Guid> userGroupIds = await _context.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.GroupId)
            .ToListAsync(cancellationToken);

        if (userGroupIds.Count == 0)
        {
            return new List<string>();
        }

        // Get all distinct user IDs from those groups, excluding the user themselves
        List<string> sharedUserIds = await _context.GroupMembers
            .Where(gm => userGroupIds.Contains(gm.GroupId) && gm.UserId != userId)
            .Select(gm => gm.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return sharedUserIds;
    }

    public async Task<List<string>> GetGroupMemberIdsAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId)
            .Select(gm => gm.UserId)
            .ToListAsync(cancellationToken);
    }
}
