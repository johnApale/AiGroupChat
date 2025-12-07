using AiGroupChat.Application.Interfaces;
using Moq;

namespace AiGroupChat.UnitTests.Services.UserService;

public abstract class UserServiceTestBase
{
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Application.Services.UserService UserService;

    protected UserServiceTestBase()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        UserService = new Application.Services.UserService(UserRepositoryMock.Object);
    }
}