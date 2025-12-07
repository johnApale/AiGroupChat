namespace AiGroupChat.Domain.Entities;

public class AiProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string? BaseUrl { get; set; }
    public string DefaultModel { get; set; } = string.Empty;
    public decimal DefaultTemperature { get; set; } = 0.7m;
    public int MaxTokensLimit { get; set; }
    public decimal InputTokenCost { get; set; }
    public decimal OutputTokenCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<AiResponseMetadata> AiResponseMetadata { get; set; } = new List<AiResponseMetadata>();
}