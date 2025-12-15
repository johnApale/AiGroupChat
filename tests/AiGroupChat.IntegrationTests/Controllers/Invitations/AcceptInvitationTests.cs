using System.Net;
using System.Net.Http.Json;
using AiGroupChat.Application.DTOs.Auth;
using AiGroupChat.Application.DTOs.Groups;
using AiGroupChat.Application.DTOs.Invitations;
using AiGroupChat.IntegrationTests.Infrastructure;

namespace AiGroupChat.IntegrationTests.Controllers.Invitations;

public class AcceptInvitationTests : IntegrationTestBase
{
    public AcceptInvitationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AcceptInvitation_WithExistingUser_AddsToGroupAndReturnsAuth()
    {
        // Arrange - Create owner and group
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create another user who will be invited
        Auth.ClearAuthToken();
        AuthResponse invitedUser = await Auth.CreateAuthenticatedUserAsync(
            email: "invited@example.com",
            userName: "invited",
            displayName: "Invited User");

        // Switch back to owner and send invitation
        Auth.SetAuthToken(owner.AccessToken);
        await Invitations.InviteMemberAsync(group.Id, "invited@example.com");

        // Get the token from the sent email
        string? invitationToken = ExtractTokenFromEmail("invited@example.com");
        Assert.NotNull(invitationToken);

        // Clear auth to simulate unauthenticated request
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AcceptInvitationResponse? result = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        Assert.NotNull(result);
        Assert.False(result.RequiresRegistration);
        Assert.NotNull(result.Auth);
        Assert.Equal(invitedUser.User.Id, result.Auth.User.Id);
        Assert.Equal(group.Id, result.GroupId);
        Assert.NotEmpty(result.Auth.AccessToken);
        Assert.NotEmpty(result.Auth.RefreshToken);

        // Verify user is now a member of the group
        Auth.SetAuthToken(result.Auth.AccessToken);
        HttpResponseMessage membersResponse = await Client.GetAsync($"/api/groups/{group.Id}/members");
        Assert.Equal(HttpStatusCode.OK, membersResponse.StatusCode);

        List<GroupMemberResponse>? members = await membersResponse.Content.ReadFromJsonAsync<List<GroupMemberResponse>>();
        Assert.NotNull(members);
        Assert.Contains(members, m => m.UserId == invitedUser.User.Id);
    }

    [Fact]
    public async Task AcceptInvitation_WithNewUser_ReturnsRequiresRegistration()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Invite a non-existent user
        await Invitations.InviteMemberAsync(group.Id, "newuser@example.com");

        // Get the token from the sent email
        string? invitationToken = ExtractTokenFromEmail("newuser@example.com");
        Assert.NotNull(invitationToken);

        // Clear auth
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AcceptInvitationResponse? result = await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        Assert.NotNull(result);
        Assert.True(result.RequiresRegistration);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.Equal("Test Group", result.GroupName);
        Assert.Null(result.Auth);
        Assert.Null(result.GroupId);
    }

    [Fact]
    public async Task AcceptInvitation_WithInvalidToken_Returns404()
    {
        // Arrange
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync("invalid-token");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_WithRevokedInvitation_Returns400()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        InviteMembersResponse inviteResult = await Invitations.InviteMemberAsync(group.Id, "user@example.com");
        Guid invitationId = inviteResult.Sent[0].Id;

        // Get token before revoking
        string? invitationToken = ExtractTokenFromEmail("user@example.com");
        Assert.NotNull(invitationToken);

        // Revoke the invitation
        await Invitations.RevokeInvitationAsync(group.Id, invitationId);

        // Clear auth
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_WhenAlreadyMember_Returns400()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create user and add as member manually
        Auth.ClearAuthToken();
        AuthResponse user = await Auth.CreateAuthenticatedUserAsync(
            email: "user@example.com",
            userName: "user",
            displayName: "Test User");

        Auth.SetAuthToken(owner.AccessToken);
        await Members.AddMemberAsync(group.Id, user.User.Id);

        // Now send invitation to the same email
        await Invitations.InviteMemberAsync(group.Id, "user@example.com");

        // Oops - the invite was rejected because user is already a member
        // Let's test a different scenario: user gets invited, then added manually, then tries to accept

        // Create a new group for this test
        GroupResponse group2 = await Groups.CreateGroupAsync("Test Group 2");

        // Invite user first
        await Invitations.InviteMemberAsync(group2.Id, "user@example.com");
        string? invitationToken = ExtractTokenFromEmail("user@example.com");
        Assert.NotNull(invitationToken);

        // Add user manually before they accept
        await Members.AddMemberAsync(group2.Id, user.User.Id);

        // Clear auth
        Auth.ClearAuthToken();

        // Act - Try to accept invitation
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_WithAlreadyAcceptedInvitation_Returns400()
    {
        // Arrange
        AuthResponse owner = await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");

        // Create user who will be invited
        Auth.ClearAuthToken();
        await Auth.CreateAuthenticatedUserAsync(
            email: "invited@example.com",
            userName: "invited",
            displayName: "Invited User");

        // Switch to owner and send invitation
        Auth.SetAuthToken(owner.AccessToken);
        await Invitations.InviteMemberAsync(group.Id, "invited@example.com");

        string? invitationToken = ExtractTokenFromEmail("invited@example.com");
        Assert.NotNull(invitationToken);

        // Clear auth and accept
        Auth.ClearAuthToken();
        await Invitations.AcceptInvitationAsync(invitationToken);

        // Act - Try to accept again
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_DoesNotRequireAuthentication()
    {
        // Arrange
        await Auth.CreateAuthenticatedUserAsync(
            email: "owner@example.com",
            userName: "owner",
            displayName: "Group Owner");

        GroupResponse group = await Groups.CreateGroupAsync("Test Group");
        await Invitations.InviteMemberAsync(group.Id, "newuser@example.com");

        string? invitationToken = ExtractTokenFromEmail("newuser@example.com");
        Assert.NotNull(invitationToken);

        // Ensure no auth token
        Auth.ClearAuthToken();

        // Act
        HttpResponseMessage response = await Invitations.AcceptInvitationRawAsync(invitationToken);

        // Assert - Should not return 401
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Extracts the invitation token from the email sent to the specified address
    /// </summary>
    private string? ExtractTokenFromEmail(string email)
    {
        Email.Models.EmailMessage? sentEmail = EmailProvider.SentEmails
            .FirstOrDefault(e => e.To == email && e.Subject.Contains("invited"));

        if (sentEmail == null)
            return null;

        // The token is in the URL as a query parameter: ?token={token}
        // Extract it from the HTML body
        string body = sentEmail.HtmlBody;
        int tokenIndex = body.IndexOf("token=", StringComparison.OrdinalIgnoreCase);
        if (tokenIndex == -1)
            return null;

        int startIndex = tokenIndex + 6; // Length of "token="
        int endIndex = body.IndexOfAny(new[] { '&', '"', '\'', '<', ' ' }, startIndex);
        if (endIndex == -1)
            endIndex = body.Length;

        string token = body.Substring(startIndex, endIndex - startIndex);

        // URL decode the token
        return System.Web.HttpUtility.UrlDecode(token);
    }
}