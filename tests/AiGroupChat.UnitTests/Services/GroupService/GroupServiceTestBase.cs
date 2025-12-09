using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public abstract class GroupServiceTestBase
{
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Mock<IAiProviderRepository> AiProviderRepositoryMock;
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<IChatHubService> ChatHubServiceMock;
    protected readonly Application.Services.GroupService GroupService;

    // Default test provider
    protected readonly AiProvider DefaultAiProvider = new()
    {
        Id = Guid.NewGuid(),
        Name = "gemini",
        DisplayName = "Google Gemini",
        IsEnabled = true,
        SortOrder = 0,
        DefaultModel = "gemini-1.5-pro",
        DefaultTemperature = 0.7m,
        MaxTokensLimit = 1000000
    };

    protected GroupServiceTestBase()
    {
        GroupRepositoryMock = new Mock<IGroupRepository>();
        AiProviderRepositoryMock = new Mock<IAiProviderRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        ChatHubServiceMock = new Mock<IChatHubService>();

        // Default: return the default provider
        AiProviderRepositoryMock
            .Setup(x => x.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DefaultAiProvider);

        GroupService = new Application.Services.GroupService(
            GroupRepositoryMock.Object,
            AiProviderRepositoryMock.Object,
            UserRepositoryMock.Object,
            ChatHubServiceMock.Object);
    }
}