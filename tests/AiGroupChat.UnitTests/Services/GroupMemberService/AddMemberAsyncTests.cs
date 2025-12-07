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
        var groupId = Guid.NewGuid();
        var currentUserId = "owner-id";
        var newUserId = "new-user-id";
        var request = new AddMemberRequest { UserId = newUserId };

        var group = new Group
        {
            Id = groupId,
            Name = "Test Group",
            Members = new List<GroupMember>()
        };

        var newUser = new User
        {
            Id = newUserId,
            UserName = "newuser",
            DisplayName = "New User"
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
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
        var result = await GroupMemberService.AddMemberAsync(groupId, request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newUserId, result.UserId);
        Assert.Equal("Member", result.Role);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var request = new AddMemberRequest { UserId = "new-user-id" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }

    [Fact]
    public async Task WithNonAdmin_ThrowsAuthorizationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var request = new AddMemberRequest { UserId = "new-user-id" };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, "current-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains("admin", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentUser_ThrowsNotFoundException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var newUserId = "nonexistent-user";
        var request = new AddMemberRequest { UserId = newUserId };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(groupId, "current-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(newUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains(newUserId, exception.Message);
    }

    [Fact]
    public async Task WithExistingMember_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var existingUserId = "existing-user";
        var request = new AddMemberRequest { UserId = existingUserId };

        var group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        var existingUser = new User { Id = existingUserId, UserName = "existing" };
        var existingMember = new GroupMember { UserId = existingUserId, User = existingUser };

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(groupId, It.IsAny<CancellationToken>()))
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
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.AddMemberAsync(groupId, request, "current-user")
        );

        Assert.Contains("already a member", exception.Message.ToLower());
    }
}