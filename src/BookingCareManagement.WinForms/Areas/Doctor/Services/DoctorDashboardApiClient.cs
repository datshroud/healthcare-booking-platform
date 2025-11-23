using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Doctor.Services.Models;

namespace BookingCareManagement.WinForms.Areas.Doctor.Services;

public sealed class DoctorDashboardApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DoctorDashboardApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DashboardSparklineResponse> GetNewCustomersAsync(string? range = null, CancellationToken cancellationToken = default)
    {
        return await GetSparklineAsync("new-customers", range, cancellationToken);
    }

    public async Task<DashboardAppointmentTrendResponse> GetAppointmentTrendAsync(string? range = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var url = string.IsNullOrWhiteSpace(range)
            ? "/api/doctor/dashboard/appointments-trend"
            : $"/api/doctor/dashboard/appointments-trend?range={range}";

        using var response = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = await response.Content.ReadFromJsonAsync<DashboardAppointmentTrendResponse>(cancellationToken: cancellationToken);
        return dto ?? new DashboardAppointmentTrendResponse();
    }

    public async Task<DashboardSparklineResponse> GetRevenueAsync(string? range = null, CancellationToken cancellationToken = default)
    {
        return await GetSparklineAsync("revenue", range, cancellationToken);
    }

    public async Task<DashboardSparklineResponse> GetOccupancyAsync(string? range = null, CancellationToken cancellationToken = default)
    {
        return await GetSparklineAsync("occupancy", range, cancellationToken);
    }

    public async Task<DashboardCustomerMixResponse> GetCustomerMixAsync(string? range = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var url = string.IsNullOrWhiteSpace(range)
            ? "/api/doctor/dashboard/customer-mix"
            : $"/api/doctor/dashboard/customer-mix?range={range}";

        using var response = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = await response.Content.ReadFromJsonAsync<DashboardCustomerMixResponse>(cancellationToken: cancellationToken);
        return dto ?? new DashboardCustomerMixResponse();
    }

    private async Task<DashboardSparklineResponse> GetSparklineAsync(string endpoint, string? range, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var url = string.IsNullOrWhiteSpace(range)
            ? $"/api/doctor/dashboard/{endpoint}"
            : $"/api/doctor/dashboard/{endpoint}?range={range}";

        using var response = await client.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = await response.Content.ReadFromJsonAsync<DashboardSparklineResponse>(cancellationToken: cancellationToken);
        return dto ?? new DashboardSparklineResponse();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException($"API returned {(int)response.StatusCode}: {content}");
        }

        throw new InvalidOperationException($"API error {(int)response.StatusCode}: {content}");
    }
}
