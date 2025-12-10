using AiGroupChat.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace AiGroupChat.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides access to helpers and handles cleanup.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly FakeEmailProvider EmailProvider;
    protected readonly AuthHelper Auth;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        EmailProvider = GetFakeEmailProvider();
        Auth = new AuthHelper(Client, EmailProvider);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await DatabaseCleaner.CleanAsync(Factory.Services);
        EmailProvider.Clear();
    }

    private FakeEmailProvider GetFakeEmailProvider()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        return (FakeEmailProvider)scope.ServiceProvider.GetRequiredService<Email.Interfaces.IEmailProvider>();
    }
}