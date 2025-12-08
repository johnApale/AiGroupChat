using AiGroupChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiGroupChat.Infrastructure.Data.Configurations;

public class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider>
{
    public void Configure(EntityTypeBuilder<AiProvider> builder)
    {
        builder.ToTable("ai_providers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(true);

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasMaxLength(500);

        builder.Property(x => x.DefaultModel)
            .HasColumnName("default_model")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DefaultTemperature)
            .HasColumnName("default_temperature")
            .HasPrecision(3, 2)
            .HasDefaultValue(0.7m);

        builder.Property(x => x.MaxTokensLimit)
            .HasColumnName("max_tokens_limit");

        builder.Property(x => x.InputTokenCost)
            .HasColumnName("input_token_cost")
            .HasPrecision(10, 6);

        builder.Property(x => x.OutputTokenCost)
            .HasColumnName("output_token_cost")
            .HasPrecision(10, 6);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}