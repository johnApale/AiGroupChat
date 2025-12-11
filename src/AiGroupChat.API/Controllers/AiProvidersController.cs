using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

/// <summary>
/// AI provider information.
/// </summary>
/// <remarks>
/// Retrieve available AI providers that can be configured for groups.
/// 
/// **Supported providers:**
/// - **Gemini** - Google's Gemini models
/// - **Claude** - Anthropic's Claude models
/// - **OpenAI** - GPT models
/// - **Grok** - xAI's Grok models
/// 
/// Each provider has different capabilities, token limits, and costs.
/// Use this endpoint to display provider options when configuring group AI settings.
/// </remarks>
[ApiController]
[Route("api/ai-providers")]
[Tags("AI Providers")]
[Authorize]
[Produces("application/json")]
public class AiProvidersController : ControllerBase
{
    private readonly IAiProviderService _aiProviderService;

    public AiProvidersController(IAiProviderService aiProviderService)
    {
        _aiProviderService = aiProviderService;
    }

    /// <summary>
    /// List available AI providers
    /// </summary>
    /// <remarks>
    /// Returns all enabled AI providers that can be used in groups.
    /// 
    /// **Response includes:**
    /// - Provider ID (use when updating group AI settings)
    /// - Display name for UI
    /// - Default model and temperature
    /// - Maximum token limit
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available AI providers</returns>
    /// <response code="200">Providers retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AiProviderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<AiProviderResponse> providers = await _aiProviderService.GetAllAsync(cancellationToken);
        return Ok(providers);
    }

    /// <summary>
    /// Get AI provider by ID
    /// </summary>
    /// <remarks>
    /// Returns details for a specific AI provider.
    /// </remarks>
    /// <param name="id">Provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI provider details</returns>
    /// <response code="200">Provider found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">Provider not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AiProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        AiProviderResponse provider = await _aiProviderService.GetByIdAsync(id, cancellationToken);
        return Ok(provider);
    }
}