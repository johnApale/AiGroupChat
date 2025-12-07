using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class CreateAsyncTests : GroupServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_CreatesGroupAndReturnsResponse()
    {
        // Arrange
        var request = new CreateGroupRequest
        {
            Name = "Test Group"
        };
        var currentUserId = "user-id-123";

        GroupRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group g, CancellationToken _) => g);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Group
            {
                Id = id,
                Name = request.Name,
                CreatedById = currentUserId,
                AiMonitoringEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Members = new List<GroupMember>
                {
                    new GroupMember
                    {
                        Id = Guid.NewGuid(),
                        GroupId = id,
                        UserId = currentUserId,
                        Role = GroupRole.Admin,
                        JoinedAt = DateTime.UtcNow,
                        User = new User
                        {
                            Id = currentUserId,
                            UserName = "testuser",
                            DisplayName = "Test User"
                        }
                    }
                }
            });

        // Act
        var result = await GroupService.CreateAsync(request, currentUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(currentUserId, result.CreatedById);
        Assert.False(result.AiMonitoringEnabled);
        Assert.Single(result.Members);
        Assert.Equal(currentUserId, result.Members[0].UserId);
        Assert.Equal("Admin", result.Members[0].Role);
    }

    [Fact]
    public async Task WithValidRequest_AddsCreatorAsAdmin()
    {
        // Arrange
        var request = new CreateGroupRequest
        {
            Name = "Test Group"
        };
        var currentUserId = "user-id-123";

        GroupRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group g, CancellationToken _) => g);

        GroupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        GroupRepositoryMock
            .Setup(x => x.GetByIdWithMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Group
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                CreatedById = currentUserId,
                Members = new List<GroupMember>()
            });

        // Act
        await GroupService.CreateAsync(request, currentUserId);

        // Assert
        GroupRepositoryMock.Verify(
            x => x.AddMemberAsync(
                It.Is<GroupMember>(m => 
                    m.UserId == currentUserId && 
                    m.Role == GroupRole.Admin),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}