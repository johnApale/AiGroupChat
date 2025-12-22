using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Application.Models;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public class RegisterAsyncTests : AuthServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_ReturnsSuccessMessageAndRequiresConfirmation()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "Password123!"
        };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        UserRepositoryMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirmation-token");

        EmailServiceMock
            .Setup(x => x.SendConfirmationEmailAsync(
                request.Email,
                request.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        RegisterResponse result = await AuthService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RequiresEmailConfirmation);
        Assert.Contains("Registration successful", result.Message);
        Assert.Null(result.Auth);
        Assert.Null(result.GroupId);
    }

    [Fact]
    public async Task WithInvalidData_ThrowsValidationException()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "weak"
        };

        string[] errors = new[] { "Password must be at least 6 characters." };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, errors));

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("Password must be at least 6 characters", exception.Message);
    }

    [Fact]
    public async Task SendsConfirmationEmail()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            DisplayName = "Test User",
            Password = "Password123!"
        };

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        UserRepositoryMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("confirmation-token");

        EmailServiceMock
            .Setup(x => x.SendConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("email-id"));

        // Act
        await AuthService.RegisterAsync(request);

        // Assert
        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                request.Email,
                request.DisplayName,
                "confirmation-token",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #region Invite-Based Registration Tests

    [Fact]
    public async Task WithValidInviteToken_CreatesUserAndAddsToGroup()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string inviteToken = "valid-invite-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = "invited@example.com",
            Token = inviteToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        UserRepositoryMock
            .Setup(x => x.ConfirmEmailDirectAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember member, CancellationToken _) => member);

        InvitationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<GroupInvitation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));

        // Act
        RegisterResponse result = await AuthService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.RequiresEmailConfirmation);
        Assert.NotNull(result.Auth);
        Assert.Equal(groupId, result.GroupId);
        Assert.Equal("access-token", result.Auth.AccessToken);
        Assert.Equal("refresh-token", result.Auth.RefreshToken);
    }

    [Fact]
    public async Task WithValidInviteToken_ConfirmsEmailDirectly()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string inviteToken = "valid-invite-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = "invited@example.com",
            Token = inviteToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        SetupTokenServiceMocks();

        // Act
        await AuthService.RegisterAsync(request);

        // Assert
        UserRepositoryMock.Verify(
            x => x.ConfirmEmailDirectAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Should NOT send confirmation email
        EmailServiceMock.Verify(
            x => x.SendConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithValidInviteToken_MarksInvitationAsAccepted()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string inviteToken = "valid-invite-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Email = "invited@example.com",
            Token = inviteToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        UserRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, Array.Empty<string>()));

        SetupTokenServiceMocks();

        // Act
        await AuthService.RegisterAsync(request);

        // Assert
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.NotNull(invitation.AcceptedAt);
        Assert.NotNull(invitation.AcceptedByUserId);

        InvitationRepositoryMock.Verify(
            x => x.UpdateAsync(invitation, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithInvalidInviteToken_ThrowsValidationException()
    {
        // Arrange
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = "invalid-token"
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupInvitation?)null);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("Invalid invitation token", exception.Message);

        // Should NOT create user
        UserRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithExpiredInviteToken_ThrowsValidationException()
    {
        // Arrange
        string inviteToken = "expired-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "invited@example.com",
            Token = inviteToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("expired", exception.Message);

        // Should NOT create user
        UserRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithAlreadyUsedInviteToken_ThrowsValidationException()
    {
        // Arrange
        string inviteToken = "used-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "invited@example.com",
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "invited@example.com",
            Token = inviteToken,
            Status = InvitationStatus.Accepted, // Already used
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("no longer valid", exception.Message);

        // Should NOT create user
        UserRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WithEmailMismatch_ThrowsValidationException()
    {
        // Arrange
        string inviteToken = "valid-token";
        
        RegisterRequest request = new RegisterRequest
        {
            Email = "different@example.com", // Different from invitation
            UserName = "inviteduser",
            DisplayName = "Invited User",
            Password = "Password123!",
            InviteToken = inviteToken
        };

        GroupInvitation invitation = new GroupInvitation
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Email = "invited@example.com", // Original invitation email
            Token = inviteToken,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        InvitationRepositoryMock
            .Setup(x => x.GetByTokenAsync(inviteToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => AuthService.RegisterAsync(request)
        );

        Assert.Contains("does not match", exception.Message);

        // Should NOT create user
        UserRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupTokenServiceMocks()
    {
        TokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token");

        TokenServiceMock
            .Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("refresh-token");

        TokenServiceMock
            .Setup(x => x.GetAccessTokenExpiration())
            .Returns(DateTime.UtcNow.AddMinutes(15));
    }

    #endregion
}