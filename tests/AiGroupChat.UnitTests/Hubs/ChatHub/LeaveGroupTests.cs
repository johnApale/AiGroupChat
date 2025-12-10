using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.LeaveGroup method
/// </summary>
public class LeaveGroupTests : ChatHubTestBase
{
    [Fact]
    public async Task LeaveGroup_RemovesFromSignalRGroup()
    {
        // Arrange - no special setup needed, LeaveGroup doesn't check membership

        // Act
        await Hub.LeaveGroup(TestGroupId);

        // Assert
        GroupsMock.Verify(
            g => g.RemoveFromGroupAsync(
                TestConnectionId,
                GetGroupName(TestGroupId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LeaveGroup_UsesCorrectGroupName()
    {
        // Arrange
        Guid specificGroupId = Guid.NewGuid();
        string expectedGroupName = $"group-{specificGroupId}";

        // Act
        await Hub.LeaveGroup(specificGroupId);

        // Assert
        GroupsMock.Verify(
            g => g.RemoveFromGroupAsync(
                TestConnectionId,
                expectedGroupName,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}