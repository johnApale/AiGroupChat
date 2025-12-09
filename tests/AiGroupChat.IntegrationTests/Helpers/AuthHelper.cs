using System.Net.Http.Headers;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for authentication-related test operations
/// </summary>
public class AuthHelper
{
    private readonly HttpClient _client;
    private readonly FakeEmailProvider _emailProvider;

    public AuthHelper(HttpClient client, FakeEmailProvider emailProvider)
    {
        _client = client;
        _emailProvider = emailProvider;
    }

    /// <summary>
    /// Registers a new user and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> RegisterAsync(
        string email = "test@example.com",
        string userName = "testuser",
        string displayName = "Test User",
        string password = "TestPass123!")
    {
        RegisterRequest request = new()
        {
            Email = email,
            UserName = userName,
            DisplayName = displayName,
            Password = password
        };

        return await _client.PostAsJsonAsync("/api/auth/register", request);
    }

    /// <summary>
    /// Confirms a user's email using the token from the last sent email
    /// </summary>
    public async Task<HttpResponseMessage> ConfirmEmailAsync(string email)
    {
        string? token = _emailProvider.ExtractTokenFromLastEmail();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Could not extract confirmation token from email");

        ConfirmEmailRequest request = new()
        {
            Email = email,
            Token = token
        };

        return await _client.PostAsJsonAsync("/api/auth/confirm-email", request);
    }

    /// <summary>
    /// Registers a user and confirms their email, returning the auth response
    /// </summary>
    public async Task<AuthResponse> RegisterAndConfirmAsync(
        string email = "test@example.com",
        string userName = "testuser",
        string displayName = "Test User",
        string password = "TestPass123!")
    {
        await RegisterAsync(email, userName, displayName, password);

        HttpResponseMessage response = await ConfirmEmailAsync(email);
        response.EnsureSuccessStatusCode();

        AuthResponse? authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse ?? throw new InvalidOperationException("Failed to deserialize auth response");
    }

    /// <summary>
    /// Logs in a user and returns the auth response
    /// </summary>
    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        LoginRequest request = new()
        {
            Email = email,
            Password = password
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        AuthResponse? authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse ?? throw new InvalidOperationException("Failed to deserialize auth response");
    }

    /// <summary>
    /// Logs in and returns just the HTTP response (for testing error cases)
    /// </summary>
    public async Task<HttpResponseMessage> LoginRawAsync(string email, string password)
    {
        LoginRequest request = new()
        {
            Email = email,
            Password = password
        };

        return await _client.PostAsJsonAsync("/api/auth/login", request);
    }

    /// <summary>
    /// Sets the authorization header with a JWT token
    /// </summary>
    public void SetAuthToken(string accessToken)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Clears the authorization header
    /// </summary>
    public void ClearAuthToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Creates a fully authenticated user and sets the auth token on the client
    /// </summary>
    public async Task<AuthResponse> CreateAuthenticatedUserAsync(
        string email = "test@example.com",
        string userName = "testuser",
        string displayName = "Test User",
        string password = "TestPass123!")
    {
        AuthResponse authResponse = await RegisterAndConfirmAsync(email, userName, displayName, password);
        SetAuthToken(authResponse.AccessToken);
        return authResponse;
    }
}