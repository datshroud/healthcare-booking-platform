using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Customer.Services.Models;

public sealed class CustomerBookingApiClient
{
 private readonly IHttpClientFactory _httpClientFactory;

 public CustomerBookingApiClient(IHttpClientFactory httpClientFactory)
 {
 _httpClientFactory = httpClientFactory;
 }

 public async Task<IReadOnlyList<SpecialtyDto>> GetSpecialtiesAsync(CancellationToken cancellationToken = default)
 {
 var client = _httpClientFactory.CreateClient("BookingCareApi");
 using var resp = await client.GetAsync("/api/customer-booking/specialties", cancellationToken);
 await EnsureSuccessAsync(resp);
 var items = await resp.Content.ReadFromJsonAsync<List<SpecialtyDto>>(cancellationToken: cancellationToken);
 return items ?? new List<SpecialtyDto>();
 }

 public async Task<IReadOnlyList<SpecialtyDoctorDto>> GetDoctorsBySpecialtyAsync(Guid specialtyId, CancellationToken cancellationToken = default)
 {
 var client = _httpClientFactory.CreateClient("BookingCareApi");
 using var resp = await client.GetAsync($"/api/customer-booking/specialties/{specialtyId}/doctors", cancellationToken);
 await EnsureSuccessAsync(resp);
 var items = await resp.Content.ReadFromJsonAsync<List<SpecialtyDoctorDto>>(cancellationToken: cancellationToken);
 return items ?? new List<SpecialtyDoctorDto>();
 }

 public async Task<IReadOnlyList<DoctorTimeSlotDto>> GetDoctorSlotsAsync(Guid doctorId, DateOnly? date = null, CancellationToken cancellationToken = default)
 {
 var client = _httpClientFactory.CreateClient("BookingCareApi");
 var url = $"/api/customer-booking/doctors/{doctorId}/time-slots" + (date.HasValue ? $"?date={date.Value:yyyy-MM-dd}" : string.Empty);
 using var resp = await client.GetAsync(url, cancellationToken);
 await EnsureSuccessAsync(resp);
 var items = await resp.Content.ReadFromJsonAsync<List<DoctorTimeSlotDto>>(cancellationToken: cancellationToken);
 return items ?? new List<DoctorTimeSlotDto>();
 }

 public async Task<AppointmentDto?> CreateAsync(CreateCustomerBookingRequest request, CancellationToken cancellationToken = default)
 {
 var client = _httpClientFactory.CreateClient("BookingCareApi");
 using var resp = await client.PostAsJsonAsync("/api/customer-booking", request, cancellationToken);
 await EnsureSuccessAsync(resp);
 return await resp.Content.ReadFromJsonAsync<AppointmentDto>(cancellationToken: cancellationToken);
 }

 private static async Task EnsureSuccessAsync(HttpResponseMessage response)
 {
 if (response.IsSuccessStatusCode) return;
 var detail = await response.Content.ReadAsStringAsync();
 throw new InvalidOperationException($"API lỗi {(int)response.StatusCode}: {detail}");
 }
}
