using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Application.DTOs.SignalR.PersonalChannel;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;

namespace AiGroupChat.Application.Services;

public class GroupMemberService : IGroupMemberService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatHubService _chatHubService;

    public GroupMemberService(
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IChatHubService chatHubService)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _chatHubService = chatHubService;
    }

    public async Task<GroupMemberResponse> AddMemberAsync(Guid groupId, AddMemberRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify current user is admin or owner
        bool isAdmin = await _groupRepository.IsAdminAsync(groupId, currentUserId, cancellationToken);
        if (!isAdmin)
        {
            throw new AuthorizationException("Only group owners and admins can add members.");
        }

        // Verify target user exists
        User? targetUser = await _userRepository.FindByIdAsync(request.UserId, cancellationToken);
        if (targetUser == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Get the current user's info for the AddedByName field
        User? currentUser = await _userRepository.FindByIdAsync(currentUserId, cancellationToken);

        // Check if user is already a member
        GroupMember? existingMember = await _groupRepository.GetMemberAsync(groupId, request.UserId, cancellationToken);
        if (existingMember != null)
        {
            throw new ValidationException("User is already a member of this group.");
        }

        DateTime now = DateTime.UtcNow;

        // Add member
        GroupMember member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            UserId = request.UserId,
            Role = GroupRole.Member,
            JoinedAt = now
        };

        await _groupRepository.AddMemberAsync(member, cancellationToken);

        // Fetch the member with user data
        GroupMember? addedMember = await _groupRepository.GetMemberAsync(groupId, request.UserId, cancellationToken);
        GroupMemberResponse response = MapToResponse(addedMember!);

        // Broadcast member joined to group (Group Channel - for active viewers)
        MemberJoinedEvent memberJoinedEvent = new MemberJoinedEvent
        {
            GroupId = groupId,
            UserId = request.UserId,
            UserName = targetUser.UserName ?? string.Empty,
            DisplayName = targetUser.DisplayName ?? string.Empty,
            Role = GroupRole.Member.ToString(),
            JoinedAt = now
        };
        await _chatHubService.BroadcastMemberJoinedAsync(groupId, memberJoinedEvent, cancellationToken);

        // Send personal channel notification to the added user
        AddedToGroupEvent addedEvent = new AddedToGroupEvent
        {
            GroupId = groupId,
            GroupName = group.Name,
            AddedByName = currentUser?.DisplayName ?? currentUser?.UserName ?? string.Empty,
            Role = GroupRole.Member.ToString(),
            AddedAt = now
        };
        await _chatHubService.SendAddedToGroupAsync(request.UserId, addedEvent, cancellationToken);

        return response;
    }

    public async Task<List<GroupMemberResponse>> GetMembersAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify current user is a member
        bool isMember = await _groupRepository.IsMemberAsync(groupId, currentUserId, cancellationToken);
        if (!isMember)
        {
            throw new AuthorizationException("You are not a member of this group.");
        }

        return group.Members.Select(MapToResponse).ToList();
    }

    public async Task<GroupMemberResponse> UpdateMemberRoleAsync(Guid groupId, string userId, UpdateMemberRoleRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify current user is owner
        bool isOwner = await _groupRepository.IsOwnerAsync(groupId, currentUserId, cancellationToken);
        if (!isOwner)
        {
            throw new AuthorizationException("Only the group owner can change member roles.");
        }

        // Get the current user's info for the ChangedByName field
        User? currentUser = await _userRepository.FindByIdAsync(currentUserId, cancellationToken);

        // Verify target member exists
        GroupMember? member = await _groupRepository.GetMemberAsync(groupId, userId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member", userId);
        }

        // Cannot change owner's role
        if (member.Role == GroupRole.Owner)
        {
            throw new ValidationException("Cannot change the owner's role. Transfer ownership instead.");
        }

        // Parse and validate role
        if (!Enum.TryParse<GroupRole>(request.Role, true, out GroupRole newRole) || newRole == GroupRole.Owner)
        {
            throw new ValidationException("Invalid role. Must be 'Admin' or 'Member'.");
        }

        string oldRole = member.Role.ToString();
        member.Role = newRole;
        await _groupRepository.UpdateMemberAsync(member, cancellationToken);

        DateTime now = DateTime.UtcNow;

        // Broadcast role change to group (Group Channel - for active viewers)
        MemberRoleChangedEvent memberRoleEvent = new MemberRoleChangedEvent
        {
            GroupId = groupId,
            UserId = userId,
            DisplayName = member.User?.DisplayName ?? member.User?.UserName ?? string.Empty,
            OldRole = oldRole,
            NewRole = newRole.ToString()
        };
        await _chatHubService.BroadcastMemberRoleChangedAsync(groupId, memberRoleEvent, cancellationToken);

        // Send personal channel notification to the user whose role changed
        RoleChangedEvent roleEvent = new RoleChangedEvent
        {
            GroupId = groupId,
            GroupName = group.Name,
            OldRole = oldRole,
            NewRole = newRole.ToString(),
            ChangedByName = currentUser?.DisplayName ?? currentUser?.UserName ?? string.Empty,
            ChangedAt = now
        };
        await _chatHubService.SendRoleChangedAsync(userId, roleEvent, cancellationToken);

        return MapToResponse(member);
    }

    public async Task RemoveMemberAsync(Guid groupId, string userId, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify target member exists
        GroupMember? member = await _groupRepository.GetMemberAsync(groupId, userId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException("Member", userId);
        }

        // Cannot remove the owner
        if (member.Role == GroupRole.Owner)
        {
            throw new ValidationException("Cannot remove the group owner. Transfer ownership first or delete the group.");
        }

        // Check authorization
        bool isOwner = await _groupRepository.IsOwnerAsync(groupId, currentUserId, cancellationToken);
        bool isAdmin = await _groupRepository.IsAdminAsync(groupId, currentUserId, cancellationToken);

        if (!isAdmin)
        {
            throw new AuthorizationException("Only group owners and admins can remove members.");
        }

        // Admin can only remove Members, not other Admins
        if (!isOwner && member.Role == GroupRole.Admin)
        {
            throw new AuthorizationException("Only the group owner can remove admins.");
        }

        // Get user display name before removal
        string displayName = member.User?.DisplayName ?? member.User?.UserName ?? string.Empty;

        await _groupRepository.RemoveMemberAsync(member, cancellationToken);

        DateTime now = DateTime.UtcNow;

        // Broadcast member left to group (Group Channel - for active viewers)
        MemberLeftEvent memberLeftEvent = new MemberLeftEvent
        {
            GroupId = groupId,
            UserId = userId,
            DisplayName = displayName,
            LeftAt = now
        };
        await _chatHubService.BroadcastMemberLeftAsync(groupId, memberLeftEvent, cancellationToken);

        // Send personal channel notification to the removed user
        RemovedFromGroupEvent removedEvent = new RemovedFromGroupEvent
        {
            GroupId = groupId,
            GroupName = group.Name,
            RemovedAt = now
        };
        await _chatHubService.SendRemovedFromGroupAsync(userId, removedEvent, cancellationToken);
    }

    public async Task LeaveGroupAsync(Guid groupId, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify current user is a member
        GroupMember? member = await _groupRepository.GetMemberAsync(groupId, currentUserId, cancellationToken);
        if (member == null)
        {
            throw new AuthorizationException("You are not a member of this group.");
        }

        // Owner cannot leave without transferring ownership
        if (member.Role == GroupRole.Owner)
        {
            throw new ValidationException("Owner cannot leave the group. Transfer ownership first or delete the group.");
        }

        // Get user display name before removal
        string displayName = member.User?.DisplayName ?? member.User?.UserName ?? string.Empty;

        await _groupRepository.RemoveMemberAsync(member, cancellationToken);

        // Broadcast member left to group
        MemberLeftEvent memberLeftEvent = new MemberLeftEvent
        {
            GroupId = groupId,
            UserId = currentUserId,
            DisplayName = displayName,
            LeftAt = DateTime.UtcNow
        };
        await _chatHubService.BroadcastMemberLeftAsync(groupId, memberLeftEvent, cancellationToken);
    }

    public async Task<GroupMemberResponse> TransferOwnershipAsync(Guid groupId, TransferOwnershipRequest request, string currentUserId, CancellationToken cancellationToken = default)
    {
        // Verify group exists
        Group? group = await _groupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group == null)
        {
            throw new NotFoundException("Group", groupId);
        }

        // Verify current user is owner
        GroupMember? currentMember = await _groupRepository.GetMemberAsync(groupId, currentUserId, cancellationToken);
        if (currentMember == null || currentMember.Role != GroupRole.Owner)
        {
            throw new AuthorizationException("Only the group owner can transfer ownership.");
        }

        // Verify new owner is a member
        GroupMember? newOwnerMember = await _groupRepository.GetMemberAsync(groupId, request.NewOwnerUserId, cancellationToken);
        if (newOwnerMember == null)
        {
            throw new NotFoundException("Member", request.NewOwnerUserId);
        }

        // Cannot transfer to yourself
        if (request.NewOwnerUserId == currentUserId)
        {
            throw new ValidationException("Cannot transfer ownership to yourself.");
        }

        // Store old roles before update
        string currentUserOldRole = currentMember.Role.ToString();
        string newOwnerOldRole = newOwnerMember.Role.ToString();

        // Transfer ownership
        currentMember.Role = GroupRole.Admin;
        newOwnerMember.Role = GroupRole.Owner;

        await _groupRepository.UpdateMemberAsync(currentMember, cancellationToken);
        await _groupRepository.UpdateMemberAsync(newOwnerMember, cancellationToken);

        // Broadcast role changes to group
        MemberRoleChangedEvent currentUserRoleEvent = new MemberRoleChangedEvent
        {
            GroupId = groupId,
            UserId = currentUserId,
            DisplayName = currentMember.User?.DisplayName ?? currentMember.User?.UserName ?? string.Empty,
            OldRole = currentUserOldRole,
            NewRole = GroupRole.Admin.ToString()
        };
        await _chatHubService.BroadcastMemberRoleChangedAsync(groupId, currentUserRoleEvent, cancellationToken);

        MemberRoleChangedEvent newOwnerRoleEvent = new MemberRoleChangedEvent
        {
            GroupId = groupId,
            UserId = request.NewOwnerUserId,
            DisplayName = newOwnerMember.User?.DisplayName ?? newOwnerMember.User?.UserName ?? string.Empty,
            OldRole = newOwnerOldRole,
            NewRole = GroupRole.Owner.ToString()
        };
        await _chatHubService.BroadcastMemberRoleChangedAsync(groupId, newOwnerRoleEvent, cancellationToken);

        // Return the new owner's member info
        GroupMember? updatedNewOwner = await _groupRepository.GetMemberAsync(groupId, request.NewOwnerUserId, cancellationToken);
        return MapToResponse(updatedNewOwner!);
    }

    private static GroupMemberResponse MapToResponse(GroupMember member)
    {
        return new GroupMemberResponse
        {
            UserId = member.UserId,
            UserName = member.User?.UserName ?? string.Empty,
            DisplayName = member.User?.DisplayName ?? string.Empty,
            Role = member.Role.ToString(),
            JoinedAt = member.JoinedAt
        };
    }
}