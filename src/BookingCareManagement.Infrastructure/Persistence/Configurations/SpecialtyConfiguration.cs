using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> e)
    {
        e.ToTable("Specialties");
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Slug).IsRequired().HasMaxLength(200);

        e.Property(x => x.Description).HasMaxLength(4000); // Cho CKEditor
        e.Property(x => x.ImageUrl).HasMaxLength(500);
        e.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("#1a73e8");
    }
}