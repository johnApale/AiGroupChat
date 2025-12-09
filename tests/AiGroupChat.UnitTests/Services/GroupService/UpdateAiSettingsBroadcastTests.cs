using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class UpdateAiSettingsBroadcastTests : GroupServiceTestBase
{
    private readonly string _userId = "user-123";
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Group _existingGroup;
    private readonly User _currentUser;

    public UpdateAiSettingsBroadcastTests()
    {
        _currentUser = new User
        {
            Id = _userId,
            UserName = "testuser",
            DisplayName = "Test User"
        };

        _existingGroup = new Group
        {
            Id = _groupId,
            Name = "Test Group",
            CreatedById = _userId,
            AiMonitoringEnabled = false,
            AiProviderId = DefaultAiProvider.Id,
            AiProvider = DefaultAiProvider,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Members = new List<GroupMember>
            {
                new() { UserId = _userId, Role = GroupRole.Owner, User = _currentUser }
            }
        };

        // Setup user repository to return current user
        UserRepositoryMock
            .Setup(x => x.FindByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_currentUser);
    }

    [Fact]
    public async Task UpdateAiSettingsAsync_BroadcastsAiSettingsChanged()
    {
        // Arrange
        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest { AiMonitoringEnabled = true };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        ChatHubServiceMock.Verify(
            x => x.BroadcastAiSettingsChangedAsync(
                _groupId,
                It.Is<AiSettingsChangedEvent>(e => 
                    e.GroupId == _groupId && 
                    e.AiMonitoringEnabled == true &&
                    e.ChangedByName == "Test User"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAiSettingsAsync_BroadcastsCorrectProviderInfo()
    {
        // Arrange
        AiProvider newProvider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "claude",
            DisplayName = "Anthropic Claude",
            IsEnabled = true,
            SortOrder = 1,
            DefaultModel = "claude-3-5-sonnet",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 200000
        };

        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest { AiProviderId = newProvider.Id };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(newProvider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newProvider);

        AiSettingsChangedEvent? capturedEvent = null;
        ChatHubServiceMock
            .Setup(x => x.BroadcastAiSettingsChangedAsync(It.IsAny<Guid>(), It.IsAny<AiSettingsChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, AiSettingsChangedEvent, CancellationToken>((_, e, _) => capturedEvent = e);

        // Act
        await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(_groupId, capturedEvent.GroupId);
        Assert.Equal(newProvider.Id, capturedEvent.AiProviderId);
        Assert.Equal("Anthropic Claude", capturedEvent.AiProviderName);
        Assert.Equal("Test User", capturedEvent.ChangedByName);
        Assert.True(capturedEvent.ChangedAt > DateTime.MinValue);
    }
}