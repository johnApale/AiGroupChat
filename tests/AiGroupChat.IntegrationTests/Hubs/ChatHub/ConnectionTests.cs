using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Helpers;
using AiGroupChat.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;

namespace AiGroupChat.IntegrationTests.Hubs.ChatHub;

/// <summary>
/// Tests for SignalR connection establishment and authentication.
/// </summary>
[Collection("SignalR")]
public class ConnectionTests : SignalRIntegrationTestBase
{
    public ConnectionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Connect_WithValidToken_Succeeds()
    {
        // Arrange
        AuthResponse authResponse = await Auth.CreateAuthenticatedUserAsync(
            email: "signalr@test.com",
            userName: "signalruser",
            displayName: "SignalR User");

        // Act
        SignalRHelper connection = await CreateSignalRConnectionAsync(authResponse.AccessToken);

        // Assert
        Assert.True(connection.IsConnected);
        Assert.Equal(HubConnectionState.Connected, connection.ConnectionState);
    }

    [Fact]
    public async Task Connect_WithInvalidToken_Fails()
    {
        // Arrange
        string invalidToken = "invalid.jwt.token";
        SignalRHelper helper = CreateSignalRHelper();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await helper.ConnectAsync(invalidToken));
    }

    [Fact]
    public async Task Connect_WithoutToken_Fails()
    {
        // Arrange
        string emptyToken = "";
        SignalRHelper helper = CreateSignalRHelper();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await helper.ConnectAsync(emptyToken));
    }
}