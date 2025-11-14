using System;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.User;
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
        e.Property(x => x.CustomerPhone).IsRequired().HasMaxLength(30);
        e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Confirmed");
        e.Property(x => x.PatientId).HasMaxLength(450);
        e.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.SetNull);
        e.HasIndex(x => new { x.DoctorId, x.StartUtc, x.EndUtc });
        e.HasIndex(x => new { x.ClinicRoomId, x.StartUtc, x.EndUtc });
    }
}
