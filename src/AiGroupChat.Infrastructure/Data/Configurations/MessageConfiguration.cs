using AiGroupChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiGroupChat.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(x => x.SenderId)
            .HasColumnName("sender_id");

        builder.Property(x => x.SenderType)
            .HasColumnName("sender_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(x => x.AiVisible)
            .HasColumnName("ai_visible");

        builder.Property(x => x.AiProviderId)
            .HasColumnName("ai_provider_id");

        builder.Property(x => x.AttachmentUrl)
            .HasColumnName("attachment_url")
            .HasMaxLength(1000);

        builder.Property(x => x.AttachmentType)
            .HasColumnName("attachment_type")
            .HasMaxLength(100);

        builder.Property(x => x.AttachmentName)
            .HasColumnName("attachment_name")
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.HasIndex(x => x.GroupId);

        builder.HasIndex(x => new { x.GroupId, x.AiVisible });

        builder.HasOne(x => x.Group)
            .WithMany(g => g.Messages)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AiProvider)
            .WithMany(p => p.Messages)
            .HasForeignKey(x => x.AiProviderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}