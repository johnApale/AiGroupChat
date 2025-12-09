using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AiGroupChat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IGroupMemberService, GroupMemberService>();
        services.AddScoped<IAiProviderService, AiProviderService>();
        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}