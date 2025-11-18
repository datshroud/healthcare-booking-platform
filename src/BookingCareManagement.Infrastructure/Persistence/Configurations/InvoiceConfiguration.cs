using BookingCareManagement.Domain.Aggregates.Invoice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingCareManagement.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        // Use a database-generated identity column for the invoice number
        builder.Property(i => i.InvoiceNumber)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.AppointmentId).IsRequired();
        builder.Property(i => i.InvoiceDate).IsRequired();
        builder.Property(i => i.Total).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.Status).HasMaxLength(50).IsRequired();

        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.AppointmentId).IsUnique();

        // relation to appointment is not configured as navigation to avoid modifying Appointment aggregate
    }
}
