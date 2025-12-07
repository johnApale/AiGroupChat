using AiGroupChat.Application.Interfaces;
using AiGroupChat.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AiGroupChat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}