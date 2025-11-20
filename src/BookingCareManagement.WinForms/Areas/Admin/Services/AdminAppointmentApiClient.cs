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
        if (!resp.IsSuccessStatusCode)
        {
            var txt = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
            {
                // Translate to a clear exception for callers
                throw new UnauthorizedAccessException($"API returned {(int)resp.StatusCode}: {txt}");
            }

            throw new InvalidOperationException($"API error {(int)resp.StatusCode}: {txt}");
        }

        var items = await resp.Content.ReadFromJsonAsync<List<CalendarEventDto>>(cancellationToken: cancellationToken);
        return items ?? new List<CalendarEventDto>();
    }
}