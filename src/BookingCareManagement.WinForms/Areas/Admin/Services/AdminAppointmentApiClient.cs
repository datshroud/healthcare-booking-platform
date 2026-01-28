using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class AdminAppointmentsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminAppointmentsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AdminAppointmentMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.GetAsync("/api/admin/appointments/metadata", cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<AdminAppointmentMetadataDto>(cancellationToken: cancellationToken)
               ?? new AdminAppointmentMetadataDto();
    }

    public async Task<IReadOnlyList<DoctorAppointmentListItemDto>> GetAppointmentsAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var query = BuildDateRangeQuery(from, to);
        var url = "/api/admin/appointments" + query;
        using var resp = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(resp);
        var items = await resp.Content.ReadFromJsonAsync<List<DoctorAppointmentListItemDto>>(cancellationToken: cancellationToken);
        return items ?? new List<DoctorAppointmentListItemDto>();
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetCalendarEventsAsync(DateOnly? from = null, DateOnly? to = null, Guid[]? doctorIds = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var query = new List<string>();
        if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");
        if (doctorIds != null && doctorIds.Length > 0)
        {
            foreach (var id in doctorIds)
            {
                query.Add($"doctorIds={id}");
            }
        }

        var url = "/api/admin/appointments/calendar" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        using var resp = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(resp);

        var items = await resp.Content.ReadFromJsonAsync<List<CalendarEventDto>>(cancellationToken: cancellationToken);
        return items ?? new List<CalendarEventDto>();
    }

    public async Task<DoctorAppointmentListItemDto?> CreateAsync(AdminAppointmentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PostAsJsonAsync("/api/admin/appointments", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task<DoctorAppointmentListItemDto?> UpdateAsync(Guid appointmentId, AdminAppointmentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PutAsJsonAsync($"/api/admin/appointments/{appointmentId}", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task<DoctorAppointmentListItemDto?> UpdateStatusAsync(Guid appointmentId, AdminAppointmentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.PostAsJsonAsync($"/api/admin/appointments/{appointmentId}/status", request, cancellationToken);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<DoctorAppointmentListItemDto>(cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        using var resp = await client.DeleteAsync($"/api/admin/appointments/{appointmentId}", cancellationToken);
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