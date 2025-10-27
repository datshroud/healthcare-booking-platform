using System;
using BookingCareManagement.Domain.Aggregates.Doctor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> e)
    {
        e.ToTable("Doctors");
        e.HasKey(x => x.Id);
        e.Property(x => x.FullName).IsRequired().HasMaxLength(200);

        e.HasMany<Specialty>() // many-to-many Doctor <-> Specialty
            .WithMany()
            .UsingEntity(j => j.ToTable("DoctorSpecialties"));
    }
}
