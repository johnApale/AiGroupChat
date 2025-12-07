using AiGroupChat.Application.Interfaces;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupService;

public abstract class GroupServiceTestBase
{
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Application.Services.GroupService GroupService;

    protected GroupServiceTestBase()
    {
        GroupRepositoryMock = new Mock<IGroupRepository>();
        GroupService = new Application.Services.GroupService(GroupRepositoryMock.Object);
    }
}