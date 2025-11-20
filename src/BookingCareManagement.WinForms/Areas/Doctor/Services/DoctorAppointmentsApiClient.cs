using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Doctor.Services;

public sealed class DoctorAppointmentsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DoctorAppointmentsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DoctorAppointmentMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.GetAsync("/api/doctor/appointments/metadata", cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentMetadataDto>(cancellationToken: cancellationToken)
               ?? new DoctorAppointmentMetadataDto();
    }

    public async Task<IReadOnlyList<DoctorAppointmentListItemDto>> GetAppointmentsAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var query = BuildDateRangeQuery(from, to);
        var url = "/api/doctor/appointments" + query;
        using var resp = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(resp);
        var items = await resp.Content.ReadFromJsonAsync<List<DoctorAppointmentListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<DoctorAppointmentListItemDto>();
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetCalendarEventsAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var query = BuildDateRangeQuery(from, to);
        var url = "/api/doctor/appointments/calendar" + query;
        using var resp = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(resp);
        var items = await resp.Content.ReadFromJsonAsync<List<CalendarEventDto>>(cancellationToken: cancellationToken);
        return items ?? new List<CalendarEventDto>();
    }

    public async Task<DoctorAppointmentListItemDto?> CreateAsync(DoctorAppointmentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PostAsJsonAsync("/api/doctor/appointments", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task<DoctorAppointmentListItemDto?> UpdateAsync(Guid appointmentId, DoctorAppointmentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PutAsJsonAsync($"/api/doctor/appointments/{appointmentId}", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task<DoctorAppointmentListItemDto?> UpdateStatusAsync(Guid appointmentId, DoctorAppointmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PostAsJsonAsync($"/api/doctor/appointments/{appointmentId}/status", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.DeleteAsync($"/api/doctor/appointments/{appointmentId}", cancellationToken);
        await EnsureSuccessAsync(resp);
    }

    private static string BuildDateRangeQuery(DateOnly? from, DateOnly? to)
    {
        var query = new List<string>();
        if (from.HasValue)
        {
            query.Add($"from={from.Value:yyyy-MM-dd}");
        }

        if (to.HasValue)
        {
            query.Add($"to={to.Value:yyyy-MM-dd}");
        }

        return query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var txt = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException($"API returned {(int)response.StatusCode}: {txt}");
        }

        throw new InvalidOperationException($"API error {(int)response.StatusCode}: {txt}");
    }
}
