using AiGroupChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiGroupChat.Infrastructure.Data.Configurations;

public class AiResponseMetadataConfiguration : IEntityTypeConfiguration<AiResponseMetadata>
{
    public void Configure(EntityTypeBuilder<AiResponseMetadata> builder)
    {
        builder.ToTable("ai_response_metadata");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.MessageId)
            .HasColumnName("message_id")
            .IsRequired();

        builder.Property(x => x.AiProviderId)
            .HasColumnName("ai_provider_id")
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TokensInput)
            .HasColumnName("tokens_input");

        builder.Property(x => x.TokensOutput)
            .HasColumnName("tokens_output");

        builder.Property(x => x.LatencyMs)
            .HasColumnName("latency_ms");

        builder.Property(x => x.CostEstimate)
            .HasColumnName("cost_estimate")
            .HasPrecision(10, 6);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(x => x.MessageId)
            .IsUnique();

        builder.HasOne(x => x.Message)
            .WithOne(m => m.AiResponseMetadata)
            .HasForeignKey<AiResponseMetadata>(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AiProvider)
            .WithMany(p => p.AiResponseMetadata)
            .HasForeignKey(x => x.AiProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}