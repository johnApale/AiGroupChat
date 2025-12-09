using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Domain.Enums;
using Moq;

namespace AiGroupChat.UnitTests.Services.MessageService;

public abstract class MessageServiceTestBase
{
    protected readonly Mock<IMessageRepository> MessageRepositoryMock;
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Application.Services.MessageService MessageService;

    protected readonly string TestUserId = "user-123";
    protected readonly Guid TestGroupId = Guid.NewGuid();

    protected readonly Group TestGroup;
    protected readonly User TestUser;

    protected MessageServiceTestBase()
    {
        MessageRepositoryMock = new Mock<IMessageRepository>();
        GroupRepositoryMock = new Mock<IGroupRepository>();

        TestUser = new User
        {
            Id = TestUserId,
            UserName = "testuser",
            DisplayName = "Test User",
            Email = "test@example.com"
        };

        TestGroup = new Group
        {
            Id = TestGroupId,
            Name = "Test Group",
            CreatedById = TestUserId,
            AiMonitoringEnabled = false,
            AiProviderId = Guid.NewGuid(),
            AiProvider = new AiProvider
            {
                Id = Guid.NewGuid(),
                Name = "gemini",
                DisplayName = "Google Gemini",
                IsEnabled = true,
                DefaultModel = "gemini-1.5-pro",
                DefaultTemperature = 0.7m,
                MaxTokensLimit = 1000000
            },
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = TestUserId,
                    Role = GroupRole.Owner,
                    User = TestUser
                }
            }
        };

        MessageService = new Application.Services.MessageService(
            MessageRepositoryMock.Object,
            GroupRepositoryMock.Object);
    }

    protected Message CreateTestMessage(Guid? id = null, string? content = null, SenderType senderType = SenderType.User)
    {
        return new Message
        {
            Id = id ?? Guid.NewGuid(),
            GroupId = TestGroupId,
            SenderId = TestUserId,
            SenderType = senderType,
            Content = content ?? "Test message",
            AiVisible = false,
            CreatedAt = DateTime.UtcNow,
            Sender = TestUser
        };
    }
}
