using System;
using System.Security.Cryptography.X509Certificates;
using BookingCareManagement.Domain.Aggregates.Appointment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> e)
    {
        e.ToTable("Appointments");
        e.HasKey(x => x.Id);
        e.Property(x => x.PatientName).IsRequired().HasMaxLength(200);
        e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Confirmed");
        e.HasIndex(x => new { x.DoctorId, x.StartUtc, x.EndUtc });
        e.HasIndex(x => new { x.ClinicRoomId, x.StartUtc, x.EndUtc });
    }
}
