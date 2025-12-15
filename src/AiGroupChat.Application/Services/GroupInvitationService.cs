using System.Security.Cryptography;
using AiGroupChat.Application.Configuration;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AiGroupChat.Application.Services;

public class GroupInvitationService : IGroupInvitationService
{
    private readonly IGroupInvitationRepository _invitationRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly InvitationSettings _invitationSettings;

    public GroupInvitationService(
        IGroupInvitationRepository invitationRepository,
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IOptions<InvitationSettings> invitationSettings)
    {
        _invitationRepository = invitationRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _invitationSettings = invitationSettings.Value;
    }

    public async Task<InviteMembersResponse> InviteMembersAsync(
        Guid groupId, 
        InviteMembersRequest request, 
        string currentUserId, 
        CancellationToken cancellationToken = default)
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
            throw new AuthorizationException("Only group owners and admins can invite members.");
        }

        InviteMembersResponse response = new InviteMembersResponse();
        DateTime now = DateTime.UtcNow;

        foreach (string email in request.Emails)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            try
            {
                // Validate email format
                if (!IsValidEmail(normalizedEmail))
                {
                    response.Failed.Add(new InvitationError
                    {
                        Email = email,
                        Reason = "Invalid email format."
                    });
                    continue;
                }

                // Check if user is already a member
                User? existingUser = await _userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);
                if (existingUser != null)
                {
                    bool isMember = await _groupRepository.IsMemberAsync(groupId, existingUser.Id, cancellationToken);
                    if (isMember)
                    {
                        response.Failed.Add(new InvitationError
                        {
                            Email = email,
                            Reason = "User is already a member of this group."
                        });
                        continue;
                    }
                }

                // Check for existing pending invitation
                GroupInvitation? existingInvitation = await _invitationRepository.GetPendingByEmailAndGroupAsync(
                    normalizedEmail, groupId, cancellationToken);

                GroupInvitation invitation;
                if (existingInvitation != null)
                {
                    // Resend: update existing invitation
                    existingInvitation.Token = GenerateSecureToken();
                    existingInvitation.ExpiresAt = now.AddDays(_invitationSettings.ExpirationDays);                    
                    existingInvitation.LastSentAt = now;
                    existingInvitation.SendCount++;
                    
                    await _invitationRepository.UpdateAsync(existingInvitation, cancellationToken);
                    invitation = existingInvitation;
                }
                else
                {
                    // Create new invitation
                    invitation = new GroupInvitation
                    {
                        Id = Guid.NewGuid(),
                        GroupId = groupId,
                        Email = normalizedEmail,
                        InvitedById = currentUserId,
                        Token = GenerateSecureToken(),
                        Status = InvitationStatus.Pending,
                        CreatedAt = now,
                        ExpiresAt = now.AddDays(_invitationSettings.ExpirationDays),
                        LastSentAt = now,
                        SendCount = 1
                    };

                    await _invitationRepository.CreateAsync(invitation, cancellationToken);
                }

                // Send invitation email
                User? inviter = await _userRepository.FindByIdAsync(currentUserId, cancellationToken);
                string inviterName = inviter?.DisplayName ?? inviter?.UserName ?? "Someone";

                await _emailService.SendGroupInvitationEmailAsync(
                    normalizedEmail,
                    group.Name,
                    inviterName,
                    invitation.Token,
                    _invitationSettings.ExpirationDays,
                    cancellationToken);
                    
                // Reload invitation with InvitedBy for response
                GroupInvitation? reloadedInvitation = await _invitationRepository.GetByIdAsync(invitation.Id, cancellationToken);
                response.Sent.Add(MapToResponse(reloadedInvitation!));
            }
            catch (Exception)
            {
                response.Failed.Add(new InvitationError
                {
                    Email = email,
                    Reason = "Failed to send invitation. Please try again."
                });
            }
        }

        return response;
    }

    public async Task<List<InvitationResponse>> GetPendingInvitationsAsync(
        Guid groupId, 
        string currentUserId, 
        CancellationToken cancellationToken = default)
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
            throw new AuthorizationException("Only group owners and admins can view pending invitations.");
        }

        List<GroupInvitation> invitations = await _invitationRepository.GetPendingByGroupAsync(groupId, cancellationToken);
        return invitations.Select(MapToResponse).ToList();
    }

    public async Task RevokeInvitationAsync(
        Guid groupId, 
        Guid invitationId, 
        string currentUserId, 
        CancellationToken cancellationToken = default)
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
            throw new AuthorizationException("Only group owners and admins can revoke invitations.");
        }

        // Get invitation
        GroupInvitation? invitation = await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
        if (invitation == null || invitation.GroupId != groupId)
        {
            throw new NotFoundException("Invitation", invitationId);
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ValidationException("Only pending invitations can be revoked.");
        }

        // Revoke
        invitation.Status = InvitationStatus.Revoked;
        invitation.RevokedAt = DateTime.UtcNow;
        invitation.RevokedById = currentUserId;

        await _invitationRepository.UpdateAsync(invitation, cancellationToken);
    }

    public async Task<AcceptInvitationResponse> AcceptInvitationAsync(
        AcceptInvitationRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Find invitation by token
        GroupInvitation? invitation = await _invitationRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (invitation == null)
        {
            throw new NotFoundException("Invitation not found or invalid token.");
        }

        // Validate invitation status
        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ValidationException("This invitation is no longer valid.");
        }

        // Check expiration
        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("This invitation has expired.");
        }

        // Check if user exists with this email
        User? existingUser = await _userRepository.FindByEmailAsync(invitation.Email, cancellationToken);

        if (existingUser == null)
        {
            // User needs to register first
            return new AcceptInvitationResponse
            {
                RequiresRegistration = true,
                Email = invitation.Email,
                GroupName = invitation.Group.Name
            };
        }

        // User exists - add them to the group
        // First check if they're already a member (edge case)
        bool isMember = await _groupRepository.IsMemberAsync(invitation.GroupId, existingUser.Id, cancellationToken);
        if (isMember)
        {
            throw new ValidationException("You are already a member of this group.");
        }

        // Add as member
        GroupMember member = new GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = invitation.GroupId,
            UserId = existingUser.Id,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        };
        await _groupRepository.AddMemberAsync(member, cancellationToken);

        // Mark invitation as accepted
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = DateTime.UtcNow;
        invitation.AcceptedByUserId = existingUser.Id;
        await _invitationRepository.UpdateAsync(invitation, cancellationToken);

        // Generate auth tokens
        string accessToken = _tokenService.GenerateAccessToken(existingUser);
        string refreshToken = await _tokenService.GenerateRefreshTokenAsync(existingUser, cancellationToken);

        return new AcceptInvitationResponse
        {
            RequiresRegistration = false,
            GroupId = invitation.GroupId,
            Auth = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetAccessTokenExpiration(),
                User = new UserDto
                {
                    Id = existingUser.Id,
                    Email = existingUser.Email ?? string.Empty,
                    UserName = existingUser.UserName ?? string.Empty,
                    DisplayName = existingUser.DisplayName
                }
            }
        };
    }

    private static string GenerateSecureToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            System.Net.Mail.MailAddress addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static InvitationResponse MapToResponse(GroupInvitation invitation)
    {
        return new InvitationResponse
        {
            Id = invitation.Id,
            GroupId = invitation.GroupId,
            Email = invitation.Email,
            Status = invitation.Status.ToString(),
            InvitedByUserName = invitation.InvitedBy?.DisplayName 
                ?? invitation.InvitedBy?.UserName 
                ?? string.Empty,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            LastSentAt = invitation.LastSentAt,
            SendCount = invitation.SendCount
        };
    }
}