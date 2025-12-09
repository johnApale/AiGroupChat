using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public class TransferOwnershipAsyncTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_TransfersOwnership()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentOwnerId = "owner-id";
        string newOwnerId = "new-owner-id";
        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = newOwnerId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        
        GroupMember currentOwner = new GroupMember
        {
            UserId = currentOwnerId,
            Role = GroupRole.Owner,
            User = new User { Id = currentOwnerId, UserName = "owner", DisplayName = "Owner" }
        };

        GroupMember newOwner = new GroupMember
        {
            UserId = newOwnerId,
            Role = GroupRole.Member,
            User = new User { Id = newOwnerId, UserName = "newowner", DisplayName = "New Owner" }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, currentOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentOwner);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, newOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOwner);

        GroupRepositoryMock
            .Setup(x => x.UpdateMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember m, CancellationToken _) => m);

        // Act
        GroupMemberResponse result = await GroupMemberService.TransferOwnershipAsync(groupId, request, currentOwnerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newOwnerId, result.UserId);
        Assert.Equal("Owner", result.Role);
        Assert.Equal(GroupRole.Admin, currentOwner.Role);
        Assert.Equal(GroupRole.Owner, newOwner.Role);
    }

    [Fact]
    public async Task WithNonOwner_ThrowsAuthorizationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string adminId = "admin-id";
        string newOwnerId = "new-owner-id";
        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = newOwnerId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        
        GroupMember admin = new GroupMember
        {
            UserId = adminId,
            Role = GroupRole.Admin,
            User = new User { Id = adminId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, adminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        // Act & Assert
        AuthorizationException exception = await Assert.ThrowsAsync<AuthorizationException>(
            () => GroupMemberService.TransferOwnershipAsync(groupId, request, adminId)
        );

        Assert.Contains("owner", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonMemberAsNewOwner_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string currentOwnerId = "owner-id";
        string nonMemberId = "non-member-id";
        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = nonMemberId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        
        GroupMember currentOwner = new GroupMember
        {
            UserId = currentOwnerId,
            Role = GroupRole.Owner,
            User = new User { Id = currentOwnerId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, currentOwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentOwner);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, nonMemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.TransferOwnershipAsync(groupId, request, currentOwnerId)
        );

        Assert.Contains(nonMemberId, exception.Message);
    }

    [Fact]
    public async Task WithTransferToSelf_ThrowsValidationException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        string ownerId = "owner-id";
        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = ownerId };

        Group group = new Group { Id = groupId, Name = "Test Group", Members = new List<GroupMember>() };
        
        GroupMember owner = new GroupMember
        {
            UserId = ownerId,
            Role = GroupRole.Owner,
            User = new User { Id = ownerId }
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        GroupRepositoryMock
            .Setup(x => x.GetMemberAsync(groupId, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
            () => GroupMemberService.TransferOwnershipAsync(groupId, request, ownerId)
        );

        Assert.Contains("yourself", exception.Message.ToLower());
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        Guid groupId = Guid.NewGuid();
        TransferOwnershipRequest request = new TransferOwnershipRequest { NewOwnerUserId = "new-owner-id" };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        NotFoundException exception = await Assert.ThrowsAsync<NotFoundException>(
            () => GroupMemberService.TransferOwnershipAsync(groupId, request, "owner-id")
        );

        Assert.Contains(groupId.ToString(), exception.Message);
    }
}