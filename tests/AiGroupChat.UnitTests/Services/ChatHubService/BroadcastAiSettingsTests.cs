using AiGroupChat.Application.DTOs.SignalR.GroupChannel;

namespace AiGroupChat.UnitTests.Services.ChatHubService;

/// <summary>
/// Tests for ChatHubService.BroadcastAiSettingsChangedAsync
/// </summary>
public class BroadcastAiSettingsTests : ChatHubServiceTestBase
{
    [Fact]
    public async Task BroadcastAiSettingsChangedAsync_SendsToCorrectGroup()
    {
        // Arrange
        AiSettingsChangedEvent settings = CreateTestSettings();

        // Act
        await ChatHubService.BroadcastAiSettingsChangedAsync(TestGroupId, settings);

        // Assert
        Assert.Equal(GetGroupName(TestGroupId), CapturedGroupName);
    }

    [Fact]
    public async Task BroadcastAiSettingsChangedAsync_SendsCorrectEventName()
    {
        // Arrange
        AiSettingsChangedEvent settings = CreateTestSettings();

        // Act
        await ChatHubService.BroadcastAiSettingsChangedAsync(TestGroupId, settings);

        // Assert
        Assert.Equal("AiSettingsChanged", CapturedMethodName);
    }

    [Fact]
    public async Task BroadcastAiSettingsChangedAsync_SendsCorrectPayload()
    {
        // Arrange
        AiSettingsChangedEvent settings = CreateTestSettings();

        // Act
        await ChatHubService.BroadcastAiSettingsChangedAsync(TestGroupId, settings);

        // Assert
        Assert.NotNull(CapturedArgs);
        Assert.Single(CapturedArgs);
        Assert.Same(settings, CapturedArgs[0]);
    }

    private AiSettingsChangedEvent CreateTestSettings()
    {
        return new AiSettingsChangedEvent
        {
            GroupId = TestGroupId,
            AiMonitoringEnabled = true,
            AiProviderId = Guid.NewGuid(),
            AiProviderName = "Google Gemini"
        };
    }
}