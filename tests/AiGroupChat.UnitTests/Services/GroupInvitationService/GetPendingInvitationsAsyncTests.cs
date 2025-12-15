using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupInvitationService;

public class GetPendingInvitationsAsyncTests : GroupInvitationServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_ReturnsInvitations()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User { Id = currentUserId, UserName = "admin", DisplayName = "Admin User" };

        List<GroupInvitation> invitations = new List<GroupInvitation>
        {
            new GroupInvitation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Email = "user1@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6),
                LastSentAt = DateTime.UtcNow.AddDays(-1),
                SendCount = 1
            },
            new GroupInvitation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Email = "user2@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                LastSentAt = DateTime.UtcNow,
                SendCount = 2
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations);

        // Act
        List<InvitationResponse> result = await GroupInvitationService.GetPendingInvitationsAsync(
            groupId, currentUserId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("user1@example.com", result[0].Email);
        Assert.Equal("user2@example.com", result[1].Email);
        Assert.Equal("Pending", result[0].Status);
        Assert.Equal(1, result[0].SendCount);
        Assert.Equal(2, result[1].SendCount);
    }

    [Fact]
    public async Task WithNoPendingInvitations_ReturnsEmptyList()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GroupInvitation>());

        // Act
        List<InvitationResponse> result = await GroupInvitationService.GetPendingInvitationsAsync(
            groupId, currentUserId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.GetPendingInvitationsAsync(groupId, currentUserId));
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "member-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupInvitationService.GetPendingInvitationsAsync(groupId, currentUserId));

        Assert.Contains("admin", exception.Message.ToLower());
    }

    [Fact]
    public async Task ReturnsCorrectInviterDisplayName()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User 
        { 
            Id = "inviter-id", 
            UserName = "inviter", 
            DisplayName = "The Inviter" 
        };

        List<GroupInvitation> invitations = new List<GroupInvitation>
        {
            new GroupInvitation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Email = "user@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                LastSentAt = DateTime.UtcNow,
                SendCount = 1
            }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByGroupAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitations);

        // Act
        List<InvitationResponse> result = await GroupInvitationService.GetPendingInvitationsAsync(
            groupId, currentUserId);

        // Assert
        Assert.Single(result);
        Assert.Equal("The Inviter", result[0].InvitedByUserName);
    }
}