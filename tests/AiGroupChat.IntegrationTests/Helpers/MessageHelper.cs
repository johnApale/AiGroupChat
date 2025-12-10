using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Common;
using AiGroupChat.Application.DTOs.Messages;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for message-related test operations
/// </summary>
public class MessageHelper
{
    private readonly HttpClient _client;

    public MessageHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Sends a message to a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> SendMessageRawAsync(Guid groupId, string content)
    {
        SendMessageRequest request = new() { Content = content };
        return await _client.PostAsJsonAsync($"/api/groups/{groupId}/messages", request);
    }

    /// <summary>
    /// Sends a message to a group and returns the response object
    /// </summary>
    public async Task<MessageResponse> SendMessageAsync(Guid groupId, string content)
    {
        HttpResponseMessage response = await SendMessageRawAsync(groupId, content);
        response.EnsureSuccessStatusCode();

        MessageResponse? message = await response.Content.ReadFromJsonAsync<MessageResponse>();
        return message ?? throw new InvalidOperationException("Failed to deserialize message response");
    }

    /// <summary>
    /// Gets messages from a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetMessagesRawAsync(Guid groupId, int page = 1, int pageSize = 50)
    {
        return await _client.GetAsync($"/api/groups/{groupId}/messages?page={page}&pageSize={pageSize}");
    }

    /// <summary>
    /// Gets messages from a group and returns the paginated response
    /// </summary>
    public async Task<PaginatedResponse<MessageResponse>> GetMessagesAsync(Guid groupId, int page = 1, int pageSize = 50)
    {
        HttpResponseMessage response = await GetMessagesRawAsync(groupId, page, pageSize);
        response.EnsureSuccessStatusCode();

        PaginatedResponse<MessageResponse>? messages = await response.Content.ReadFromJsonAsync<PaginatedResponse<MessageResponse>>();
        return messages ?? throw new InvalidOperationException("Failed to deserialize messages response");
    }
}