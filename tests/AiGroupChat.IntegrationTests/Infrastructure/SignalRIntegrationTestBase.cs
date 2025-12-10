using AiGroupChat.IntegrationTests.Helpers;

namespace AiGroupChat.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for SignalR integration tests.
/// Provides factory method for creating SignalR connections with automatic cleanup.
/// 
/// All test classes inheriting from this should use [Collection("SignalR")] to ensure
/// sequential execution and shared factory instance.
/// </summary>
public abstract class SignalRIntegrationTestBase : IntegrationTestBase
{
    private readonly List<SignalRHelper> _signalRConnections = new();

    protected SignalRIntegrationTestBase(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    /// <summary>
    /// Creates a new SignalR connection with the given access token.
    /// The connection is automatically tracked and disposed after the test.
    /// </summary>
    /// <param name="accessToken">JWT access token for authentication</param>
    /// <returns>A connected SignalRHelper instance</returns>
    protected async Task<SignalRHelper> CreateSignalRConnectionAsync(string accessToken)
    {
        string hubUrl = GetHubUrl();
        HttpMessageHandler handler = Factory.Server.CreateHandler();
        SignalRHelper connection = new SignalRHelper(hubUrl, handler);
        await connection.ConnectAsync(accessToken);
        _signalRConnections.Add(connection);
        return connection;
    }

    /// <summary>
    /// Creates a SignalRHelper without connecting.
    /// Useful for testing connection failure scenarios.
    /// The helper is still tracked for disposal.
    /// </summary>
    protected SignalRHelper CreateSignalRHelper()
    {
        string hubUrl = GetHubUrl();
        HttpMessageHandler handler = Factory.Server.CreateHandler();
        SignalRHelper helper = new SignalRHelper(hubUrl, handler);
        _signalRConnections.Add(helper);
        return helper;
    }

    public override async Task DisposeAsync()
    {
        // Dispose all SignalR connections first
        foreach (SignalRHelper connection in _signalRConnections)
        {
            await connection.DisposeAsync();
        }
        _signalRConnections.Clear();

        // Then call base cleanup (database, email provider)
        await base.DisposeAsync();
    }

    private string GetHubUrl()
    {
        // Factory.Server.BaseAddress includes trailing slash
        Uri baseAddress = Factory.Server.BaseAddress;
        return $"{baseAddress}hubs/chat";
    }
}