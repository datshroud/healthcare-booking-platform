using BookingCareManagement.Domain.Aggregates.SupportChat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class SupportConversationConfiguration : IEntityTypeConfiguration<SupportConversation>
{
    public void Configure(EntityTypeBuilder<SupportConversation> builder)
    {
        builder.ToTable("SupportConversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.StaffId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.StaffRole)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DoctorId)
            .IsRequired(false);

        builder.Property(x => x.Subject)
            .HasMaxLength(64)
            .HasDefaultValue("general");

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.CustomerId, x.StaffId })
            .IsUnique();

        builder.HasIndex(x => x.StaffId);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Staff)
            .WithMany()
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
