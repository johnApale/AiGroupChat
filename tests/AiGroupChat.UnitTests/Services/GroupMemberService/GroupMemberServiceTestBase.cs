using AiGroupChat.Application.Interfaces;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupMemberService;

public abstract class GroupMemberServiceTestBase
{
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<IChatHubService> ChatHubServiceMock;
    protected readonly Application.Services.GroupMemberService GroupMemberService;

    protected GroupMemberServiceTestBase()
    {
        GroupRepositoryMock = new Mock<IGroupRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        ChatHubServiceMock = new Mock<IChatHubService>();
        GroupMemberService = new Application.Services.GroupMemberService(
            GroupRepositoryMock.Object,
            UserRepositoryMock.Object,
            ChatHubServiceMock.Object
        );
    }
}