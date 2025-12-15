using AiGroupChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiGroupChat.Infrastructure.Data.Configurations;

public class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.ToTable("group_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.InvitedById)
            .HasColumnName("invited_by_id")
            .IsRequired();

        builder.Property(x => x.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Tracking timestamps
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(x => x.LastSentAt)
            .HasColumnName("last_sent_at");

        builder.Property(x => x.SendCount)
            .HasColumnName("send_count")
            .HasDefaultValue(1);

        // Acceptance tracking
        builder.Property(x => x.AcceptedAt)
            .HasColumnName("accepted_at");

        builder.Property(x => x.AcceptedByUserId)
            .HasColumnName("accepted_by_user_id");

        // Revocation tracking
        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(x => x.RevokedById)
            .HasColumnName("revoked_by_id");

        // Indexes
        builder.HasIndex(x => x.Token)
            .IsUnique();

        // Partial unique index: only one pending invitation per email per group
        builder.HasIndex(x => new { x.GroupId, x.Email })
            .IsUnique()
            .HasFilter("status = 'Pending'");

        // Relationships
        builder.HasOne(x => x.Group)
            .WithMany(g => g.Invitations)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InvitedBy)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(x => x.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcceptedByUser)
            .WithMany(u => u.AcceptedInvitations)
            .HasForeignKey(x => x.AcceptedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RevokedBy)
            .WithMany(u => u.RevokedInvitations)
            .HasForeignKey(x => x.RevokedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}