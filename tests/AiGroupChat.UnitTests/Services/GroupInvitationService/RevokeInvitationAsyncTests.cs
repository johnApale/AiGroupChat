using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupInvitationService;

public class RevokeInvitationAsyncTests : GroupInvitationServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_RevokesInvitation()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        GroupInvitation invitation = new GroupInvitation
        {
            Id = invitationId,
            GroupId = groupId,
            Email = "user@example.com",
            Status = InvitationStatus.Pending
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(invitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act
        await GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId);

        // Assert
        InvitationRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<GroupInvitation>(i =>
                i.Status == InvitationStatus.Revoked &&
                i.RevokedById == currentUserId &&
                i.RevokedAt != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
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
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));

        Assert.Contains("admin", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentInvitation_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(invitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));
    }

    [Fact]
    public async Task WithInvitationFromDifferentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid otherGroupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        GroupInvitation invitation = new GroupInvitation
        {
            Id = invitationId,
            GroupId = otherGroupId, // Different group
            Email = "user@example.com",
            Status = InvitationStatus.Pending
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(invitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));
    }

    [Fact]
    public async Task WithAlreadyAcceptedInvitation_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        GroupInvitation invitation = new GroupInvitation
        {
            Id = invitationId,
            GroupId = groupId,
            Email = "user@example.com",
            Status = InvitationStatus.Accepted
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(invitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));

        Assert.Contains("pending", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithAlreadyRevokedInvitation_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        Guid invitationId = Guid.NewGuid();
        string currentUserId = "admin-id";

        Group group = new Group { Id = groupId, Name = "Test Group" };
        GroupInvitation invitation = new GroupInvitation
        {
            Id = invitationId,
            GroupId = groupId,
            Email = "user@example.com",
            Status = InvitationStatus.Revoked
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(invitationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.RevokeInvitationAsync(groupId, invitationId, currentUserId));

        Assert.Contains("pending", exception.Message.ToLower());
    }
}