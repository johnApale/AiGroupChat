using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class AddMemberAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_AddsMemberAndReturnsResponse()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentUserId = "owner-id";
        string newUserId = "new-user-id";
        AddMemberRequest request = new AddMemberRequest { UserId = newUserId };

        Group group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        User newUser = new User
        {
            Id = newUserId,
            UserName = "newuser",
            DisplayName = "New User"
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        GroupRepositoryMock
            .SetupSequence(x => x.GetMemberAsync(groupId, newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null)
            .ReturnsAsync(new GroupMember
            {
                UserId = newUserId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
                User = newUser
            });

        // Act
        GroupMemberResponse result = await GroupMemberService.AddMemberAsync(groupId, request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newUserId, result.UserId);
        Assert.Equal("Member", result.Role);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        AddMemberRequest request = new AddMemberRequest { UserId = "new-user-id" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        AddMemberRequest request = new AddMemberRequest { UserId = "new-user-id" };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, "current-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains("admin", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentUser_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string newUserId = "nonexistent-user";
        AddMemberRequest request = new AddMemberRequest { UserId = newUserId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, "current-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains(newUserId, exception.Message);
    }

    [Fact]
    public async Task WithExistingMember_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string existingUserId = "existing-user";
        AddMemberRequest request = new AddMemberRequest { UserId = existingUserId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        User existingUser = new User { Id = existingUserId, UserName = "existing" };
        GroupMember existingMember = new GroupMember { UserId = existingUserId, User = existingUser };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, "current-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(existingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, existingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMember);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains("already a member", exception.Message.ToLower());
    }
}