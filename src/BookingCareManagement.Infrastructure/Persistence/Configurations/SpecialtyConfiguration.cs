using System;
using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Data.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> e)
    {
        e.ToTable("Specialties");
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).IsRequired().HasMaxLength(100);
        e.Property(x => x.Slug).IsRequired().HasMaxLength(150);
        e.HasIndex(x => x.Slug).IsUnique();
    }
}
