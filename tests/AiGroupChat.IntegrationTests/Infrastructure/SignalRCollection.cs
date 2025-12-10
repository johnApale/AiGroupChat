namespace AiGroupChat.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition for SignalR integration tests.
/// All test classes decorated with [Collection("SignalR")] will:
/// 1. Share the same CustomWebApplicationFactory instance
/// 2. Run sequentially (not in parallel) to avoid state conflicts
/// </summary>
[CollectionDefinition("SignalR")]
public class SignalRCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class has no code - it's just a marker for xUnit
    // to understand that all [Collection("SignalR")] classes
    // share the same factory and run sequentially
}