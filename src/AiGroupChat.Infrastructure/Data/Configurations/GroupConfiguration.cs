using AiGroupChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiGroupChat.Infrastructure.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CreatedById)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(x => x.AiMonitoringEnabled)
            .HasColumnName("ai_monitoring_enabled")
            .HasDefaultValue(false);

        builder.Property(x => x.AiProviderId)
            .HasColumnName("ai_provider_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.CreatedBy)
            .WithMany(u => u.CreatedGroups)
            .HasForeignKey(x => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AiProvider)
            .WithMany(p => p.Groups)
            .HasForeignKey(x => x.AiProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}