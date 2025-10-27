using System;
using BookingCareManagement.Domain.Aggregates.ClinicRoom;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class ClinicRoomConfiguration : IEntityTypeConfiguration<ClinicRoom>
{
    public void Configure(EntityTypeBuilder<ClinicRoom> e)
    {
        e.ToTable("ClinicRooms");
        e.HasKey(x => x.Id);
        e.Property(x => x.Code).IsRequired().HasMaxLength(50);
        e.HasIndex(x => x.Code).IsUnique();
        e.Property(x => x.Capacity).HasDefaultValue(1);
    }
}
