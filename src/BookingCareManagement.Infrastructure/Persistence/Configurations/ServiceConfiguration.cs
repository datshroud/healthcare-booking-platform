using System;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> e)
    {
        e.ToTable("Services");
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        e.Property(x => x.Price).HasPrecision(10, 2);

        e.HasMany<Specialty>() // many-to-many Service <-> Specialty
            .WithMany()
            .UsingEntity(j => j.ToTable("ServiceSpecialties"));
    }
}
