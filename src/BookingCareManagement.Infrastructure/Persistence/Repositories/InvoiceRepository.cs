using System.Globalization;
using BookingCareManagement.Application.Features.Invoices.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Invoice;
using BookingCareManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingCareManagement.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDBContext _context;
    private readonly ILogger<InvoiceRepository> _logger;
    private const decimal DefaultServicePrice = 200.00m; // fallback price when no pricing data available

    public InvoiceRepository(ApplicationDBContext context, ILogger<InvoiceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureInvoicesForAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        // Find appointments that do not yet have an invoice
        var appointmentsWithoutInvoice = await _context.Appointments
            .Where(a => !_context.Invoices.Any(i => i.AppointmentId == a.Id))
            .ToListAsync(cancellationToken);

        if (!appointmentsWithoutInvoice.Any()) return;

        var specialtyIds = appointmentsWithoutInvoice
            .Select(a => a.SpecialtyId)
            .Distinct()
            .ToList();

        var specialtyPrices = await _context.Specialties
            .Where(s => specialtyIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Price, cancellationToken);

        foreach (var appt in appointmentsWithoutInvoice)
        {
            var specialtyPrice = specialtyPrices.TryGetValue(appt.SpecialtyId, out var priceFromSpecialty)
                ? priceFromSpecialty
                : DefaultServicePrice;

            var total = appt.Price > 0 ? appt.Price : specialtyPrice;

            var invoice = new Invoice(appt.Id, total, invoiceDate: DateTime.UtcNow);
            _context.Invoices.Add(invoice);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<InvoiceListDto>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        // Build a projection that EF can translate, then map to DTOs in memory
        var raw = await (from inv in _context.Invoices.AsNoTracking()
                         join appt in _context.Appointments.AsNoTracking() on inv.AppointmentId equals appt.Id
                         join spec in _context.Specialties.AsNoTracking() on appt.SpecialtyId equals spec.Id into specg
                         from spec in specg.DefaultIfEmpty()
                         join user in _context.Users.AsNoTracking() on appt.PatientId equals user.Id into userg
                         from user in userg.DefaultIfEmpty()
                         select new
                         {
                             InvId = inv.Id,
                             inv.InvoiceNumber,
                             AppointmentId = appt.Id,
                             InvoiceDate = inv.InvoiceDate,
                             inv.Total,
                             inv.Status,
                             AppointmentPatientName = appt.PatientName,
                             UserFullName = user != null ? user.FullName : null,
                             UserEmail = user != null ? user.Email : null,
                             ServiceName = spec != null ? spec.Name : null,
                             ServiceQty = 1
                         })
                        .OrderByDescending(x => x.InvoiceNumber)
                        .ToListAsync(cancellationToken);

        var result = raw.Select(x => new InvoiceListDto
        {
            Id = x.InvId,
            InvoiceNumber = x.InvoiceNumber,
            AppointmentId = x.AppointmentId,
            InvoiceDate = x.InvoiceDate,
            Total = x.Total,
            Status = x.Status,
            CustomerName = !string.IsNullOrWhiteSpace(x.UserFullName) ? x.UserFullName! : x.AppointmentPatientName,
            CustomerEmail = x.UserEmail ?? string.Empty,
            ServiceName = x.ServiceName ?? string.Empty,
            ServiceQty = x.ServiceQty,
            ServicePriceDisplay = x.Total.ToString("C", new System.Globalization.CultureInfo("vi-VN"))
        }).ToList();

        return result;
    }

    public async Task<bool> MarkAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        const string newStatus = "Paid";

        // First try EF Core ExecuteUpdateAsync (server-side update without tracking)
        try
        {
            var rows = await _context.Invoices
                .Where(i => i.Id == invoiceId)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, _ => newStatus), cancellationToken);

            if (rows > 0) return true;

            // if no rows updated, invoice not found
            _logger.LogInformation("MarkAsPaid: invoice {InvoiceId} not found via ExecuteUpdate", invoiceId);
            return false;
        }
        catch (Exception ex)
        {
            // Log and fall back to raw SQL update - this increases chance of success in constrained environments
            _logger.LogWarning(ex, "ExecuteUpdateAsync failed for invoice {InvoiceId}, falling back to raw SQL", invoiceId);

            try
            {
                var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE [Invoices] SET [Status] = {newStatus} WHERE [Id] = {invoiceId}", cancellationToken);
                if (rows > 0) return true;

                _logger.LogInformation("MarkAsPaid: invoice {InvoiceId} not found via raw SQL", invoiceId);
                return false;
            }
            catch (DbUpdateException dbex)
            {
                _logger.LogError(dbex, "Failed to mark invoice {InvoiceId} as paid (raw SQL)", invoiceId);
                throw;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Unexpected error while marking invoice {InvoiceId} as paid (fallback)", invoiceId);
                throw new DbUpdateException("Failed to mark invoice as paid", fallbackEx);
            }
        }
    }
}
