using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Customer.Models;

namespace BookingCareManagement.WinForms.Areas.Customer.Services
{
    public sealed class CustomerBookingService
    {
        private readonly IHttpClientFactory _httpFactory;

        private sealed class BookingListResponse
        {
            public CustomerBookingDto[]? Items { get; set; }
        }

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
                var payload = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(payload)) return Array.Empty<CustomerBookingDto>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                try
                {
                    var wrapper = JsonSerializer.Deserialize<BookingListResponse>(payload, options);
                    if (wrapper?.Items != null)
                    {
                        return wrapper.Items;
                    }
                }
                catch
                {
                    // ignore and fall back
                }

                return JsonSerializer.Deserialize<CustomerBookingDto[]>(payload, options);
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
