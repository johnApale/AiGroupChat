using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Invitations;

namespace AiGroupChat.IntegrationTests.Helpers;

/// <summary>
/// Helper methods for group invitation test operations
/// </summary>
public class GroupInvitationHelper
{
    private readonly HttpClient _client;

    public GroupInvitationHelper(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Invites members to a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> InviteMembersRawAsync(Guid groupId, List<string> emails)
    {
        InviteMembersRequest request = new() { Emails = emails };
        return await _client.PostAsJsonAsync($"/api/groups/{groupId}/invitations", request);
    }

    /// <summary>
    /// Invites members to a group and returns the response object
    /// </summary>
    public async Task<InviteMembersResponse> InviteMembersAsync(Guid groupId, List<string> emails)
    {
        HttpResponseMessage response = await InviteMembersRawAsync(groupId, emails);
        response.EnsureSuccessStatusCode();

        InviteMembersResponse? result = await response.Content.ReadFromJsonAsync<InviteMembersResponse>();
        return result ?? throw new InvalidOperationException("Failed to deserialize invite response");
    }

    /// <summary>
    /// Invites a single member to a group
    /// </summary>
    public async Task<InviteMembersResponse> InviteMemberAsync(Guid groupId, string email)
    {
        return await InviteMembersAsync(groupId, new List<string> { email });
    }

    /// <summary>
    /// Gets pending invitations for a group and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> GetPendingInvitationsRawAsync(Guid groupId)
    {
        return await _client.GetAsync($"/api/groups/{groupId}/invitations");
    }

    /// <summary>
    /// Gets pending invitations for a group and returns the list
    /// </summary>
    public async Task<List<InvitationResponse>> GetPendingInvitationsAsync(Guid groupId)
    {
        HttpResponseMessage response = await GetPendingInvitationsRawAsync(groupId);
        response.EnsureSuccessStatusCode();

        List<InvitationResponse>? result = await response.Content.ReadFromJsonAsync<List<InvitationResponse>>();
        return result ?? throw new InvalidOperationException("Failed to deserialize invitations response");
    }

    /// <summary>
    /// Revokes an invitation and returns the HTTP response
    /// </summary>
    public async Task<HttpResponseMessage> RevokeInvitationRawAsync(Guid groupId, Guid invitationId)
    {
        return await _client.DeleteAsync($"/api/groups/{groupId}/invitations/{invitationId}");
    }

    /// <summary>
    /// Revokes an invitation
    /// </summary>
    public async Task RevokeInvitationAsync(Guid groupId, Guid invitationId)
    {
        HttpResponseMessage response = await RevokeInvitationRawAsync(groupId, invitationId);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Accepts an invitation and returns the HTTP response (uses public endpoint)
    /// </summary>
    public async Task<HttpResponseMessage> AcceptInvitationRawAsync(string token)
    {
        AcceptInvitationRequest request = new() { Token = token };
        return await _client.PostAsJsonAsync("/api/invitations/accept", request);
    }

    /// <summary>
    /// Accepts an invitation and returns the response object
    /// </summary>
    public async Task<AcceptInvitationResponse> AcceptInvitationAsync(string token)
    {
        HttpResponseMessage response = await AcceptInvitationRawAsync(token);
        response.EnsureSuccessStatusCode();

        AcceptInvitationResponse? result = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        return result ?? throw new InvalidOperationException("Failed to deserialize accept response");
    }
}