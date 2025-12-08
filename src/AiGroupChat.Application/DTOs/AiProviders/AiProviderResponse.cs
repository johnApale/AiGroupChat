namespace AiGroupChat.Application.DTOs.AiProviders;

public class AiProviderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    public decimal DefaultTemperature { get; set; }
    public int MaxTokensLimit { get; set; }
}
