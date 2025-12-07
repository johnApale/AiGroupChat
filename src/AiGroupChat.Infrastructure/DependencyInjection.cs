using AiGroupChat.Application;
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

        // Repositories
        services.AddScoped<IUserRepository, IdentityUserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();

        // Services
        services.AddScoped<ITokenService, TokenService>();

        // Application layer services
        services.AddApplication();

        // Email
        services.AddEmail(configuration);

        return services;
    }
}