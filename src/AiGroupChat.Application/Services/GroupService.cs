using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;

    public GroupService(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<GroupResponse> CreateAsync(CreateGroupRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedById = currentUserId,
            AiMonitoringEnabled = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _groupRepository.CreateAsync(group, cancellationToken);

        // Add creator as admin
        var member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = currentUserId,
            Role = GroupRole.Admin,
            JoinedAt = now
        };

        await _groupRepository.AddMemberAsync(member, cancellationToken);

        // Fetch the group with members to return
        var createdGroup = await _groupRepository.GetByIdWithMembersAsync(group.Id, cancellationToken);
        return MapToResponse(createdGroup!);
    }

    public async Task<List<GroupResponse>> GetMyGroupsAsync(string currentUserId, CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetGroupsByUserIdAsync(currentUserId, cancellationToken);
        return groups.Select(MapToResponse).ToList();
    }

    public async Task<GroupResponse> GetByIdAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        var isMember = await _groupRepository.IsMemberAsync(groupId, currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new AuthorizationException("You are not a member of this group.");
        }

        return MapToResponse(group);
    }

    public async Task<GroupResponse> UpdateAsync(Guid groupId, UpdateGroupRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        var isAdmin = await _groupRepository.IsAdminAsync(groupId, currentUserId, cancellationToken);

        if (!isAdmin)
        {
            throw new AuthorizationException("Only group admins can update the group.");
        }

        group.Name = request.Name;
        group.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.UpdateAsync(group, cancellationToken);

        return MapToResponse(group);
    }

    public async Task DeleteAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        var isAdmin = await _groupRepository.IsAdminAsync(groupId, currentUserId, cancellationToken);

        if (!isAdmin)
        {
            throw new AuthorizationException("Only group admins can delete the group.");
        }

        await _groupRepository.DeleteAsync(group, cancellationToken);
    }

    private static GroupResponse MapToResponse(Group group)
    {
        return new GroupResponse
        {
            Id = group.Id,
            Name = group.Name,
            CreatedById = group.CreatedById,
            AiMonitoringEnabled = group.AiMonitoringEnabled,
            AiProviderId = group.AiProviderId,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = group.Members.Select(m => new GroupMemberResponse
            {
                UserId = m.UserId,
                UserName = m.User?.UserName ?? string.Empty,
                DisplayName = m.User?.DisplayName ?? string.Empty,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList()
        };
    }
}