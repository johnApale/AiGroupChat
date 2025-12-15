using AiGroupChat.Application;
using AiGroupChat.Application.Configuration;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Email;
using AiGroupChat.Infrastructure.Configuration;
using AiGroupChat.Infrastructure.Data;
using AiGroupChat.Infrastructure.Repositories;
using AiGroupChat.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiGroupChat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        // Identity
        services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings (using defaults)
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AiServiceSettings>(configuration.GetSection(AiServiceSettings.SectionName));
        services.Configure<InvitationSettings>(configuration.GetSection(InvitationSettings.SectionName));

        // Repositories
        services.AddScoped<IUserRepository, IdentityUserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IAiResponseMetadataRepository, AiResponseMetadataRepository>();
        services.AddScoped<IGroupInvitationRepository, GroupInvitationRepository>();
    
        // Services
        services.AddScoped<ITokenService, TokenService>();

        // AI Client Service with typed HttpClient
        AiServiceSettings aiSettings = configuration.GetSection(AiServiceSettings.SectionName).Get<AiServiceSettings>() ?? new AiServiceSettings();
        services.AddHttpClient<IAiClientService, AiClientService>(client =>
        {
            client.BaseAddress = new Uri(aiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(aiSettings.TimeoutSeconds);
        });

        // Application layer services
        services.AddApplication();

        // Email
        services.AddEmail(configuration);

        return services;
    }
}