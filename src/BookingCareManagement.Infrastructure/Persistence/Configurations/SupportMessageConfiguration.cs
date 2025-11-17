using BookingCareManagement.Domain.Aggregates.SupportChat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> builder)
    {
        builder.ToTable("SupportMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasMaxLength(512);

        builder.Property(x => x.Author)
            .HasConversion<int>();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
