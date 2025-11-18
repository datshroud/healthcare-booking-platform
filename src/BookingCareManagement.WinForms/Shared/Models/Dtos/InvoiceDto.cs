using System;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed class InvoiceDto
{
    public Guid Id { get; set; }
    public int InvoiceNumber { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;

    // Related display fields
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int ServiceQty { get; set; } = 1;
    public string ServicePriceDisplay { get; set; } = string.Empty;
}
