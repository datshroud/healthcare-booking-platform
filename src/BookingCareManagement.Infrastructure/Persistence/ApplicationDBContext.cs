using System;
using BookingCareManagement.Domain.Aggregates.Appointment;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using BookingCareManagement.Domain.Aggregates.Doctor;
using BookingCareManagement.Domain.Aggregates.Service;
using BookingCareManagement.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace BookingCareManagement.Infrastructure.Persistence;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> o) : base(o) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ClinicRoom> ClinicRooms => Set<ClinicRoom>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);

        base.OnModelCreating(mb);

        mb.Entity<AppUser>(cfg =>
        {
            cfg.OwnsMany(u => u.RefreshTokens, r =>
            {
                r.WithOwner().HasForeignKey("AppUserId");
                r.Property<int>("Id");
                r.HasKey("Id");
                r.Property(x => x.Token).HasMaxLength(256);
            });
        });
    }
}
