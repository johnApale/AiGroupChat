using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AiGroupChat.UnitTests.Hubs.ChatHub;

/// <summary>
/// Tests for the ChatHub.JoinGroup method
/// </summary>
public class JoinGroupTests : ChatHubTestBase
{
    [Fact]
    public async Task JoinGroup_WhenUserIsMember_AddsToSignalRGroup()
    {
        // Arrange
        SetupUserIsMember(isMember: true);

        // Act
        await Hub.JoinGroup(TestGroupId);

        // Assert
        GroupsMock.Verify(
            g => g.AddToGroupAsync(
                TestConnectionId,
                GetGroupName(TestGroupId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinGroup_WhenUserIsNotMember_ThrowsHubException()
    {
        // Arrange
        SetupUserIsMember(isMember: false);

        // Act & Assert
        HubException exception = await Assert.ThrowsAsync<HubException>(
            () => Hub.JoinGroup(TestGroupId));

        Assert.Equal("You are not a member of this group.", exception.Message);
    }

    [Fact]
    public async Task JoinGroup_WhenUserIsNotMember_DoesNotAddToGroup()
    {
        // Arrange
        SetupUserIsMember(isMember: false);

        // Act
        try
        {
            await Hub.JoinGroup(TestGroupId);
        }
        catch (HubException)
        {
            // Expected
        }

        // Assert - AddToGroupAsync should never be called
        GroupsMock.Verify(
            g => g.AddToGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinGroup_VerifiesMembershipWithCorrectParameters()
    {
        // Arrange
        SetupUserIsMember(isMember: true);

        // Act
        await Hub.JoinGroup(TestGroupId);

        // Assert - Verify IsMemberAsync was called with correct group and user
        GroupRepositoryMock.Verify(
            r => r.IsMemberAsync(
                TestGroupId,
                TestUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}