using AiGroupChat.Application.DTOs.AiProviders;
using AiGroupChat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiGroupChat.API.Controllers;

[ApiController]
[Route("api/ai-providers")]
[Authorize]
public class AiProvidersController : ControllerBase
{
    private readonly IAiProviderService _aiProviderService;

    public AiProvidersController(IAiProviderService aiProviderService)
    {
        _aiProviderService = aiProviderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        List<AiProviderResponse> providers = await _aiProviderService.GetAllAsync(cancellationToken);
        return Ok(providers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        AiProviderResponse provider = await _aiProviderService.GetByIdAsync(id, cancellationToken);
        return Ok(provider);
    }
}
