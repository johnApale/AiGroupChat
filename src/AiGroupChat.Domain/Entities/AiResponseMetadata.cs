namespace AiGroupChat.Domain.Entities;

public class AiResponseMetadata
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid AiProviderId { get; set; }
    public string Model { get; set; } = string.Empty;
    public int TokensInput { get; set; }
    public int TokensOutput { get; set; }
    public int LatencyMs { get; set; }
    public decimal? CostEstimate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Message Message { get; set; } = null!;
    public AiProvider AiProvider { get; set; } = null!;
}