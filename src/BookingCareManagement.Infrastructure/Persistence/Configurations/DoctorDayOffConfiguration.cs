using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class DoctorDayOffConfiguration : IEntityTypeConfiguration<DoctorDayOff>
{
    public void Configure(EntityTypeBuilder<DoctorDayOff> builder)
    {
        builder.ToTable("DoctorDayOffs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.StartDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.RepeatYearly)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.DoctorId, x.Name })
            .IsUnique();
    }
}
