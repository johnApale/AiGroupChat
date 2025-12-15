using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupInvitationService;

public class InviteMembersAsyncTests : GroupInvitationServiceTestBase
{
    [Fact]
    public async Task WithValidEmails_CreatesInvitationsAndSendsEmails()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { "user1@example.com", "user2@example.com" }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User { Id = currentUserId, DisplayName = "Admin User" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviter);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByEmailAndGroupAsync(It.IsAny<string>(), groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        InvitationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation inv, CancellationToken _) => inv);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new GroupInvitation
            {
                Id = id,
                GroupId = groupId,
                Email = "test@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter
            });

        EmailServiceMock
            .Setup(x => x.SendGroupInvitationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("test-email-id"));

        // Act
        InviteMembersResponse result = await GroupInvitationService.InviteMembersAsync(
            groupId, request, currentUserId);

        // Assert
        Assert.Equal(2, result.Sent.Count);
        Assert.Empty(result.Failed);

        InvitationRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        EmailServiceMock.Verify(
            x => x.SendGroupInvitationEmailAsync(
                It.IsAny<string>(), "Test Group", "Admin User",
                It.IsAny<string>(), 7, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { "user@example.com" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.InviteMembersAsync(groupId, request, "user-id"));
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "member-id";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { "user@example.com" }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupInvitationService.InviteMembersAsync(groupId, request, currentUserId));

        Assert.Contains("admin", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithInvalidEmail_ReturnsFailedResult()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { "invalid-email", "valid@example.com" }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User { Id = currentUserId, DisplayName = "Admin" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync("valid@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviter);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByEmailAndGroupAsync("valid@example.com", groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        InvitationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation inv, CancellationToken _) => inv);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new GroupInvitation
            {
                Id = id,
                GroupId = groupId,
                Email = "valid@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter
            });

        EmailServiceMock
            .Setup(x => x.SendGroupInvitationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("test-email-id"));

        // Act
        InviteMembersResponse result = await GroupInvitationService.InviteMembersAsync(
            groupId, request, currentUserId);

        // Assert
        Assert.Single(result.Sent);
        Assert.Single(result.Failed);
        Assert.Equal("invalid-email", result.Failed[0].Email);
        Assert.Contains("invalid", result.Failed[0].Reason.ToLower());
    }

    [Fact]
    public async Task WithExistingMember_ReturnsFailedResult()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";
        string existingMemberEmail = "existing@example.com";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { existingMemberEmail }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User existingUser = new User { Id = "existing-user-id", Email = existingMemberEmail };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(existingMemberEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(groupId, existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        InviteMembersResponse result = await GroupInvitationService.InviteMembersAsync(
            groupId, request, currentUserId);

        // Assert
        Assert.Empty(result.Sent);
        Assert.Single(result.Failed);
        Assert.Contains("already a member", result.Failed[0].Reason.ToLower());
    }

    [Fact]
    public async Task WithExistingPendingInvitation_ResendsAndUpdatesInvitation()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";
        string email = "user@example.com";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { email }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User { Id = currentUserId, DisplayName = "Admin" };

        GroupInvitation existingInvitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = email,
            Token = "old-token",
            SendCount = 1,
            Status = InvitationStatus.Pending,
            InvitedBy = inviter
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviter);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByEmailAndGroupAsync(email, groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitation);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(existingInvitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvitation);

        EmailServiceMock
            .Setup(x => x.SendGroupInvitationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("test-email-id"));

        // Act
        InviteMembersResponse result = await GroupInvitationService.InviteMembersAsync(
            groupId, request, currentUserId);

        // Assert
        Assert.Single(result.Sent);
        Assert.Empty(result.Failed);

        InvitationRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<GroupInvitation>(i => 
                i.SendCount == 2 && i.Token != "old-token"), 
                It.IsAny<CancellationToken>()),
            Times.Once);

        InvitationRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task NormalizesEmailToLowercase()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "admin-id";
        InviteMembersRequest request = new InviteMembersRequest
        {
            Emails = new List<string> { "USER@EXAMPLE.COM" }
        };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User inviter = new User { Id = currentUserId, DisplayName = "Admin" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inviter);

        InvitationRepositoryMock
            .Setup(x => x.GetPendingByEmailAndGroupAsync("user@example.com", groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        InvitationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation inv, CancellationToken _) => inv);

        InvitationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new GroupInvitation
            {
                Id = id,
                GroupId = groupId,
                Email = "user@example.com",
                Status = InvitationStatus.Pending,
                InvitedBy = inviter
            });

        EmailServiceMock
            .Setup(x => x.SendGroupInvitationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("test-email-id"));
        // Act
        await GroupInvitationService.InviteMembersAsync(groupId, request, currentUserId);

        // Assert
        InvitationRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<GroupInvitation>(i => i.Email == "user@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
