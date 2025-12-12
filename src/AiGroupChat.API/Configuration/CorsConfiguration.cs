namespace AiGroupChat.API.Configuration;

/// <summary>
/// CORS configuration for the API.
/// </summary>
public static class CorsConfiguration
{
    public const string PolicyName = "AllowFrontend";

    /// <summary>
    /// Adds CORS services with frontend development origins.
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();  // Required for SignalR
            });
        });

        return services;
    }

    /// <summary>
    /// Applies the CORS middleware. Must be called before UseAuthentication.
    /// </summary>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}