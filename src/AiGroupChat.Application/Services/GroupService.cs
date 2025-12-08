using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IAiProviderRepository _aiProviderRepository;

    public GroupService(IGroupRepository groupRepository, IAiProviderRepository aiProviderRepository)
    {
        _groupRepository = groupRepository;
        _aiProviderRepository = aiProviderRepository;
    }

    public async Task<GroupResponse> CreateAsync(CreateGroupRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Get default AI provider (first enabled by sort order)
        var defaultProvider = await _aiProviderRepository.GetDefaultAsync(cancellationToken)
            ?? throw new ValidationException("No AI providers are available. Please contact an administrator.");

        var now = DateTime.UtcNow;

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedById = currentUserId,
            AiMonitoringEnabled = false,
            AiProviderId = defaultProvider.Id,
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
            Role = GroupRole.Owner,
            JoinedAt = now
        };

        await _groupRepository.AddMemberAsync(member, cancellationToken);

        // Fetch the group with members to return
        var createdGroup = await _groupRepository.GetByIdAsync(group.Id, cancellationToken);
        return MapToResponse(createdGroup!);
    }

    public async Task<List<GroupResponse>> GetMyGroupsAsync(string currentUserId, CancellationToken cancellationToken = default)
    {
        var groups = await _groupRepository.GetGroupsByUserIdAsync(currentUserId, cancellationToken);
        return groups.Select(MapToResponse).ToList();
    }

    public async Task<GroupResponse> GetByIdAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

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
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

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
        var group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);

        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        var isOwner = await _groupRepository.IsOwnerAsync(groupId, currentUserId, cancellationToken);

        if (!isOwner)
        {
            throw new AuthorizationException("Only the group owner can delete the group.");
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
            AiProvider = new AiProviderResponse
            {
                Id = group.AiProvider.Id,
                Name = group.AiProvider.Name,
                DisplayName = group.AiProvider.DisplayName,
                DefaultModel = group.AiProvider.DefaultModel,
                DefaultTemperature = group.AiProvider.DefaultTemperature,
                MaxTokensLimit = group.AiProvider.MaxTokensLimit
            },
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