using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Messages;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.MessageService;

public class GetMessagesAsyncTests : MessageServiceTestBase
{
    [Fact]
    public async Task WithValidRequest_ReturnsMessages()
    {
        // Arrange
        List<Message> messages = new List<Message>
        {
            CreateTestMessage(content: "Message 1"),
            CreateTestMessage(content: "Message 2"),
            CreateTestMessage(content: "Message 3")
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.GetByGroupIdAsync(TestGroupId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        MessageRepositoryMock
            .Setup(x => x.GetCountByGroupIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        PaginatedResponse<MessageResponse> result = await MessageService.GetMessagesAsync(TestGroupId, TestUserId);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(50, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        List<Message> messages = new List<Message>
        {
            CreateTestMessage(content: "Message on page 2")
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.GetByGroupIdAsync(TestGroupId, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        MessageRepositoryMock
            .Setup(x => x.GetCountByGroupIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        // Act
        PaginatedResponse<MessageResponse> result = await MessageService.GetMessagesAsync(TestGroupId, TestUserId, page: 2, pageSize: 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task WithEmptyGroup_ReturnsEmptyList()
    {
        // Arrange
        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.GetByGroupIdAsync(TestGroupId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        MessageRepositoryMock
            .Setup(x => x.GetCountByGroupIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        PaginatedResponse<MessageResponse> result = await MessageService.GetMessagesAsync(TestGroupId, TestUserId);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            MessageService.GetMessagesAsync(TestGroupId, TestUserId));
    }

    [Fact]
    public async Task WithNonMember_ThrowsAuthorizationException()
    {
        // Arrange
        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() =>
            MessageService.GetMessagesAsync(TestGroupId, TestUserId));
    }

    [Fact]
    public async Task WithInvalidPageSize_ClampsToMaximum()
    {
        // Arrange
        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestGroup);

        GroupRepositoryMock
            .Setup(x => x.IsMemberAsync(TestGroupId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        MessageRepositoryMock
            .Setup(x => x.GetByGroupIdAsync(TestGroupId, 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        MessageRepositoryMock
            .Setup(x => x.GetCountByGroupIdAsync(TestGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        PaginatedResponse<MessageResponse> result = await MessageService.GetMessagesAsync(TestGroupId, TestUserId, page: 1, pageSize: 500);

        // Assert
        Assert.Equal(100, result.PageSize);
        MessageRepositoryMock.Verify(x => x.GetByGroupIdAsync(TestGroupId, 1, 100, It.IsAny<CancellationToken>()), Times.Once);
    }
}
