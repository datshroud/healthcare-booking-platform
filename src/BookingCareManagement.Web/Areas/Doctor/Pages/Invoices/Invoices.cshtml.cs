using BookingCareManagement.Application.Features.Invoices.Dtos;
using BookingCareManagement.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingCareManagement.Areas.Doctor.Pages.Invoices
{
    public class InvoicesModel : PageModel
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoicesModel(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public List<InvoiceListDto> Invoices { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Ensure invoices exist for appointments
            await _invoiceRepository.EnsureInvoicesForAppointmentsAsync();

            var items = await _invoiceRepository.GetAllWithDetailsAsync();
            Invoices = items.ToList();
        }
    }
}
