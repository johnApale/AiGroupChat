using Scalar.AspNetCore;

namespace AiGroupChat.API.Configuration;

public static class OpenApiConfiguration
{
public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
{
    services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new()
            {
                Title = "AI Group Chat API",
                Version = "v1",
                Description = """
                    A group chat application with integrated AI agent capabilities.
                    
                    ## Features
                    - **Authentication** - JWT-based auth with refresh tokens and email verification
                    - **Groups** - Create and manage chat groups with role-based permissions
                    - **Real-time Messaging** - REST API for sending, SignalR for receiving
                    - **AI Integration** - Multiple AI providers (Gemini, Claude, OpenAI, Grok)
                    
                    ## Authentication
                    Most endpoints require a valid JWT token. Include it in the `Authorization` header:
                    ```
                    Authorization: Bearer <your-access-token>
                    ```     
                    ## Rate Limiting
                    Currently no rate limiting is enforced (MVP).
                    """,
                Contact = new()
                {
                    Name = "AI Group Chat Support",
                    Email = "support@example.com"
                }
            };
            return Task.CompletedTask;
        });
    });

    return services;
}
    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("AI Group Chat API")
                    .WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
            });
        }

        return app;
    }
}