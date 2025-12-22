using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Services;
using Moq;

namespace AiGroupChat.UnitTests.Services.AuthService;

public abstract class AuthServiceTestBase
{
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<ITokenService> TokenServiceMock;
    protected readonly Mock<IEmailService> EmailServiceMock;
    protected readonly Mock<IGroupInvitationRepository> InvitationRepositoryMock;
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Application.Services.AuthService AuthService;

    protected AuthServiceTestBase()
    {
        UserRepositoryMock = new Mock<IUserRepository>();
        TokenServiceMock = new Mock<ITokenService>();
        EmailServiceMock = new Mock<IEmailService>();
        InvitationRepositoryMock = new Mock<IGroupInvitationRepository>();
        GroupRepositoryMock = new Mock<IGroupRepository>();

        AuthService = new Application.Services.AuthService(
            UserRepositoryMock.Object,
            TokenServiceMock.Object,
            EmailServiceMock.Object,
            InvitationRepositoryMock.Object,
            GroupRepositoryMock.Object
        );
    }
}