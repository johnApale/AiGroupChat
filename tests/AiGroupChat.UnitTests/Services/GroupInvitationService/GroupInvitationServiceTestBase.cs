using AiGroupChat.Application.Configuration;
using AiGroupChat.Application.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace AiGroupChat.UnitTests.Services.GroupInvitationService;

public abstract class GroupInvitationServiceTestBase
{
    protected readonly Mock<IGroupInvitationRepository> InvitationRepositoryMock;
    protected readonly Mock<IGroupRepository> GroupRepositoryMock;
    protected readonly Mock<IUserRepository> UserRepositoryMock;
    protected readonly Mock<ITokenService> TokenServiceMock;
    protected readonly Mock<IEmailService> EmailServiceMock;
    protected readonly InvitationSettings InvitationSettings;
    protected readonly Application.Services.GroupInvitationService GroupInvitationService;

    protected GroupInvitationServiceTestBase()
    {
        InvitationRepositoryMock = new Mock<IGroupInvitationRepository>();
        GroupRepositoryMock = new Mock<IGroupRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        TokenServiceMock = new Mock<ITokenService>();
        EmailServiceMock = new Mock<IEmailService>();
        
        InvitationSettings = new InvitationSettings
        {
            ExpirationDays = 7
        };

        Mock<IOptions<InvitationSettings>> optionsMock = new Mock<IOptions<InvitationSettings>>();
        optionsMock.Setup(x => x.Value).Returns(InvitationSettings);

        GroupInvitationService = new Application.Services.GroupInvitationService(
            InvitationRepositoryMock.Object,
            GroupRepositoryMock.Object,
            UserRepositoryMock.Object,
            TokenServiceMock.Object,
            EmailServiceMock.Object,
            optionsMock.Object
        );
    }
}