using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Groups;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for group member-related test operations
/// </summary>
public class GroupMemberHelper
{
    private readonly HttpClient _client;

    public GroupMemberHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Adds a member to a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> AddMemberRawAsync(Guid groupId, string userId)
    {
        AddMemberRequest request = new() { UserId = userId };
        return await _client.PostAsJsonAsync($"/api/groups/{groupId}/members", request);
    }

    /// <summary>
    /// Adds a member to a group and returns the response object
    /// </summary>
    public async Task<GroupMemberResponse> AddMemberAsync(Guid groupId, string userId)
    {
        HttpResponseMessage response = await AddMemberRawAsync(groupId, userId);
        response.EnsureSuccessStatusCode();

        GroupMemberResponse? member = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        return member ?? throw new InvalidOperationException("Failed to deserialize member response");
    }

    /// <summary>
    /// Gets all members of a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetMembersRawAsync(Guid groupId)
    {
        return await _client.GetAsync($"/api/groups/{groupId}/members");
    }

    /// <summary>
    /// Gets all members of a group and returns the response list
    /// </summary>
    public async Task<List<GroupMemberResponse>> GetMembersAsync(Guid groupId)
    {
        HttpResponseMessage response = await GetMembersRawAsync(groupId);
        response.EnsureSuccessStatusCode();

        List<GroupMemberResponse>? members = await response.Content.ReadFromJsonAsync<List<GroupMemberResponse>>();
        return members ?? throw new InvalidOperationException("Failed to deserialize members response");
    }

    /// <summary>
    /// Updates a member's role and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> UpdateMemberRoleRawAsync(Guid groupId, string memberId, string role)
    {
        UpdateMemberRoleRequest request = new() { Role = role };
        return await _client.PutAsJsonAsync($"/api/groups/{groupId}/members/{memberId}", request);
    }

    /// <summary>
    /// Updates a member's role and returns the response object
    /// </summary>
    public async Task<GroupMemberResponse> UpdateMemberRoleAsync(Guid groupId, string memberId, string role)
    {
        HttpResponseMessage response = await UpdateMemberRoleRawAsync(groupId, memberId, role);
        response.EnsureSuccessStatusCode();

        GroupMemberResponse? member = await response.Content.ReadFromJsonAsync<GroupMemberResponse>();
        return member ?? throw new InvalidOperationException("Failed to deserialize member response");
    }

    /// <summary>
    /// Removes a member from a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> RemoveMemberRawAsync(Guid groupId, string memberId)
    {
        return await _client.DeleteAsync($"/api/groups/{groupId}/members/{memberId}");
    }

    /// <summary>
    /// Leaves a group (current user) and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> LeaveGroupRawAsync(Guid groupId)
    {
        return await _client.DeleteAsync($"/api/groups/{groupId}/members/me");
    }
}