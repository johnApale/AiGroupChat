using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.SignalR.GroupChannel;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Integration tests for AI settings change broadcasts
/// </summary>
[Collection("SignalR")]
public class AiSettingsEventTests : SignalRIntegrationTestBase
{
    public AiSettingsEventTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateAiSettings_JoinedMembersReceiveEvent()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "aiowner@test.com",
            userName: "aiowner");

        GroupResponse group = await Groups.CreateGroupAsync("AI Settings Group");

        SignalRHelper connection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - Enable AI monitoring
        await Groups.UpdateAiSettingsAsync(group.Id, aiMonitoringEnabled: true);

        // Assert
        AiSettingsChangedEvent settingsEvent = await connection.WaitForAiSettingsChangedEventAsync(
            e => e.GroupId == group.Id);

        Assert.True(settingsEvent.AiMonitoringEnabled);
    }

    [Fact]
    public async Task UpdateAiSettings_EventContainsAllFields()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "aifields@test.com",
            userName: "aifields");

        GroupResponse group = await Groups.CreateGroupAsync("AI Fields Group");

        // Get the first available AI provider
        List<Application.DTOs.AiProviders.AiProviderResponse> providers = await AiProviders.GetAllAsync();
        Application.DTOs.AiProviders.AiProviderResponse provider = providers.First();

        SignalRHelper connection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - Update AI settings with specific provider
        await Groups.UpdateAiSettingsAsync(
            group.Id, 
            aiMonitoringEnabled: true, 
            aiProviderId: provider.Id);

        // Assert
        AiSettingsChangedEvent settingsEvent = await connection.WaitForAiSettingsChangedEventAsync(
            e => e.GroupId == group.Id);

        Assert.Equal(group.Id, settingsEvent.GroupId);
        Assert.True(settingsEvent.AiMonitoringEnabled);
        Assert.Equal(provider.Id, settingsEvent.AiProviderId);
        Assert.Equal(provider.DisplayName, settingsEvent.AiProviderName);
    }

    [Fact]
    public async Task UpdateAiSettings_DisableMonitoring_BroadcastsChange()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "aidisable@test.com",
            userName: "aidisable");

        GroupResponse group = await Groups.CreateGroupAsync("AI Disable Group");

        // First enable AI
        await Groups.UpdateAiSettingsAsync(group.Id, aiMonitoringEnabled: true);

        SignalRHelper connection = await CreateSignalRConnectionAsync(owner.AccessToken);
        await connection.JoinGroupAsync(group.Id);

        // Act - Disable AI monitoring
        await Groups.UpdateAiSettingsAsync(group.Id, aiMonitoringEnabled: false);

        // Assert
        AiSettingsChangedEvent settingsEvent = await connection.WaitForAiSettingsChangedEventAsync(
            e => e.GroupId == group.Id && !e.AiMonitoringEnabled);

        Assert.False(settingsEvent.AiMonitoringEnabled);
    }

    [Fact]
    public async Task UpdateAiSettings_NonJoinedMembers_DoNotReceive()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "ainotjoined@test.com",
            userName: "ainotjoined");

        GroupResponse group = await Groups.CreateGroupAsync("AI Not Joined Group");

        // Connect but don't join the SignalR group
        SignalRHelper connection = await CreateSignalRConnectionAsync(owner.AccessToken);
        // Intentionally not calling: await connection.JoinGroupAsync(group.Id);

        // Act
        await Groups.UpdateAiSettingsAsync(group.Id, aiMonitoringEnabled: true);

        // Assert - Should NOT receive the event
        await Task.Delay(500);
        Assert.Empty(connection.AiSettingsChangedEvents);
    }
}