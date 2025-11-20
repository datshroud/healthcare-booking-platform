using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Customer.Models;

namespace BookingCareManagement.WinForms.Areas.Customer.Services
{
    public sealed class CustomerBookingService
    {
        private readonly IHttpClientFactory _httpFactory;

        public CustomerBookingService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<CustomerBookingDto[]?> GetMyBookingsAsync()
        {
            try
            {
                var client = _httpFactory.CreateClient("BookingCareApi");
                var resp = await client.GetAsync("api/customer-booking/my-bookings?filter=all");
                if (!resp.IsSuccessStatusCode) return null;
                var items = await resp.Content.ReadFromJsonAsync<CustomerBookingDto[]>();
                return items;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CancelBookingAsync(Guid appointmentId)
        {
            try
            {
                var client = _httpFactory.CreateClient("BookingCareApi");
                var resp = await client.PostAsync($"api/customer-booking/{appointmentId}/cancel", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
