using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for group-related test operations
/// </summary>
public class GroupHelper
{
    private readonly HttpClient _client;

    public GroupHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Creates a group and returns the response
    /// </summary>
    public async Task<GroupResponse> CreateGroupAsync(string name = "Test Group")
    {
        CreateGroupRequest request = new() { Name = name };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/groups", request);
        response.EnsureSuccessStatusCode();

        GroupResponse? group = await response.Content.ReadFromJsonAsync<GroupResponse>();
        return group ?? throw new InvalidOperationException("Failed to deserialize group response");
    }

    /// <summary>
    /// Creates a group and returns the HTTP response (for testing error cases)
    /// </summary>
    public async Task<HttpResponseMessage> CreateGroupRawAsync(string name)
    {
        CreateGroupRequest request = new() { Name = name };
        return await _client.PostAsJsonAsync("/api/groups", request);
    }

    /// <summary>
    /// Gets a group by ID and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetGroupRawAsync(Guid groupId)
    {
        return await _client.GetAsync($"/api/groups/{groupId}");
    }

    /// <summary>
    /// Gets all groups for the current user
    /// </summary>
    public async Task<HttpResponseMessage> GetMyGroupsRawAsync()
    {
        return await _client.GetAsync("/api/groups");
    }

    /// <summary>
    /// Updates a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> UpdateGroupRawAsync(Guid groupId, string name)
    {
        UpdateGroupRequest request = new() { Name = name };
        return await _client.PutAsJsonAsync($"/api/groups/{groupId}", request);
    }

    /// <summary>
    /// Deletes a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> DeleteGroupRawAsync(Guid groupId)
    {
        return await _client.DeleteAsync($"/api/groups/{groupId}");
    }

    /// <summary>
    /// Updates AI settings for a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> UpdateAiSettingsRawAsync(Guid groupId, bool? aiMonitoringEnabled = null, Guid? aiProviderId = null)
    {
        UpdateAiSettingsRequest request = new()
        {
            AiMonitoringEnabled = aiMonitoringEnabled,
            AiProviderId = aiProviderId
        };
        return await _client.PutAsJsonAsync($"/api/groups/{groupId}/ai", request);
    }
}