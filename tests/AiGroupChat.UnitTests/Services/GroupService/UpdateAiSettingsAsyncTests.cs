using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.Exceptions;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public class UpdateAiSettingsAsyncTests : GroupServiceTestBase
{
    private readonly string _userId = "user-123";
    private readonly Guid _groupId = Guid.NewGuid();
    private readonly Group _existingGroup;

    public UpdateAiSettingsAsyncTests()
    {
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
                new() { UserId = _userId, Role = GroupRole.Owner, User = new User { Id = _userId, UserName = "testuser", DisplayName = "Test User" } }
            }
        };
    }

    [Fact]
    public async Task WithValidRequest_UpdatesAiMonitoringEnabled()
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
        GroupResponse result = await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        Assert.True(result.AiMonitoringEnabled);
        GroupRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Group>(g => g.AiMonitoringEnabled == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithValidRequest_UpdatesAiProviderId()
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

        // Act
        GroupResponse result = await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        Assert.Equal(newProvider.Id, result.AiProviderId);
        Assert.Equal(newProvider.DisplayName, result.AiProvider.DisplayName);
    }

    [Fact]
    public async Task WithBothFields_UpdatesBoth()
    {
        // Arrange
        AiProvider newProvider = new AiProvider
        {
            Id = Guid.NewGuid(),
            Name = "openai",
            DisplayName = "OpenAI",
            IsEnabled = true,
            SortOrder = 2,
            DefaultModel = "gpt-4o",
            DefaultTemperature = 0.7m,
            MaxTokensLimit = 128000
        };

        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest
        {
            AiMonitoringEnabled = true,
            AiProviderId = newProvider.Id
        };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(newProvider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newProvider);

        // Act
        GroupResponse result = await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        Assert.True(result.AiMonitoringEnabled);
        Assert.Equal(newProvider.Id, result.AiProviderId);
    }

    [Fact]
    public async Task WithNonexistentGroup_ThrowsNotFoundException()
    {
        // Arrange
        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest { AiMonitoringEnabled = true };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            GroupService.UpdateAiSettingsAsync(_groupId, request, _userId));
    }

    [Fact]
    public async Task WithNonAdminUser_ThrowsAuthorizationException()
    {
        // Arrange
        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest { AiMonitoringEnabled = true };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() =>
            GroupService.UpdateAiSettingsAsync(_groupId, request, _userId));
    }

    [Fact]
    public async Task WithInvalidProviderId_ThrowsValidationException()
    {
        // Arrange
        Guid invalidProviderId = Guid.NewGuid();
        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest { AiProviderId = invalidProviderId };

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        AiProviderRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AiProvider?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            GroupService.UpdateAiSettingsAsync(_groupId, request, _userId));
    }

    [Fact]
    public async Task WithEmptyRequest_DoesNotChangeValues()
    {
        // Arrange
        bool originalMonitoring = _existingGroup.AiMonitoringEnabled;
        Guid? originalProviderId = _existingGroup.AiProviderId;
        UpdateAiSettingsRequest request = new UpdateAiSettingsRequest(); // Both null

        GroupRepositoryMock
            .Setup(x => x.GetByIdAsync(_groupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingGroup);

        GroupRepositoryMock
            .Setup(x => x.IsAdminAsync(_groupId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        GroupResponse result = await GroupService.UpdateAiSettingsAsync(_groupId, request, _userId);

        // Assert
        Assert.Equal(originalMonitoring, result.AiMonitoringEnabled);
        Assert.Equal(originalProviderId, result.AiProviderId);
        GroupRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
