using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Features.Invoices.Dtos;

namespace BookingCareManagement.Domain.Abstractions;

public interface IInvoiceRepository
{
    Task<IEnumerable<InvoiceListDto>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task EnsureInvoicesForAppointmentsAsync(CancellationToken cancellationToken = default);
    Task<bool> MarkAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
