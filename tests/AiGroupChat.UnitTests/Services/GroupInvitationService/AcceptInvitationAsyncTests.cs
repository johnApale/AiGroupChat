using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupInvitationService;

public class AcceptInvitationAsyncTests : GroupInvitationServiceTestBase
{
    [Fact]
    public async Task WithExistingUser_AddsToGroupAndReturnsAuth()
    {
        // Arrange
        string token = "valid-token";
        Guid groupId = Guid.NewGuid();
        string email = "user@example.com";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User existingUser = new User
        {
            Id = "user-id",
            Email = email,
            UserName = "testuser",
            DisplayName = "Test User"
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = email,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Group = group
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(groupId, existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(existingUser))
            .Returns("access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(existingUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        AcceptInvitationResponse result = await GroupInvitationService.AcceptInvitationAsync(request);

        // Assert
        Assert.False(result.RequiresRegistration);
        Assert.NotNull(result.Auth);
        Assert.Equal("access-token", result.Auth.AccessToken);
        Assert.Equal("refresh-token", result.Auth.RefreshToken);
        Assert.Equal(existingUser.Id, result.Auth.User.Id);
        Assert.Equal(groupId, result.GroupId);

        GroupRepositoryMock.Verify(
            x => x.AddMemberAsync(It.Is<GroupMember>(m =>
                m.GroupId == groupId &&
                m.UserId == existingUser.Id &&
                m.Role == GroupRole.Member),
                It.IsAny<CancellationToken>()),
            Times.Once);

        InvitationRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<GroupInvitation>(i =>
                i.Status == InvitationStatus.Accepted &&
                i.AcceptedByUserId == existingUser.Id &&
                i.AcceptedAt != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithNewUser_ReturnsRequiresRegistration()
    {
        // Arrange
        string token = "valid-token";
        Guid groupId = Guid.NewGuid();
        string email = "newuser@example.com";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        Group group = new Group { Id = groupId, Name = "Test Group" };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = email,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Group = group
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        AcceptInvitationResponse result = await GroupInvitationService.AcceptInvitationAsync(request);

        // Assert
        Assert.True(result.RequiresRegistration);
        Assert.Equal(email, result.Email);
        Assert.Equal("Test Group", result.GroupName);
        Assert.Null(result.Auth);
        Assert.Null(result.GroupId);

        // Should NOT add member or update invitation
        GroupRepositoryMock.Verify(
            x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()),
            Times.Never);

        InvitationRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithInvalidToken_ThrowsNotFoundException()
    {
        // Arrange
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = "invalid-token" };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => GroupInvitationService.AcceptInvitationAsync(request));
    }

    [Fact]
    public async Task WithExpiredInvitation_ThrowsValidationException()
    {
        // Arrange
        string token = "expired-token";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "user@example.com",
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            Group = new Group { Name = "Test Group" }
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.AcceptInvitationAsync(request));

        Assert.Contains("expired", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithAcceptedInvitation_ThrowsValidationException()
    {
        // Arrange
        string token = "accepted-token";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "user@example.com",
            Token = token,
            Status = InvitationStatus.Accepted,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Group = new Group { Name = "Test Group" }
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.AcceptInvitationAsync(request));

        Assert.Contains("no longer valid", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithRevokedInvitation_ThrowsValidationException()
    {
        // Arrange
        string token = "revoked-token";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "user@example.com",
            Token = token,
            Status = InvitationStatus.Revoked,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Group = new Group { Name = "Test Group" }
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.AcceptInvitationAsync(request));

        Assert.Contains("no longer valid", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithUserAlreadyMember_ThrowsValidationException()
    {
        // Arrange
        string token = "valid-token";
        Guid groupId = Guid.NewGuid();
        string email = "user@example.com";
        AcceptInvitationRequest request = new AcceptInvitationRequest { Token = token };

        Group group = new Group { Id = groupId, Name = "Test Group" };
        User existingUser = new User
        {
            Id = "user-id",
            Email = email,
            UserName = "testuser"
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = email,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Group = group
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(groupId, existingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupInvitationService.AcceptInvitationAsync(request));

        Assert.Contains("already a member", exception.Message.ToLower());
    }
}