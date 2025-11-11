using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Models;
using Microsoft.Extensions.Logging;

namespace BookingCareManagement.WinForms.Clients;

/// <summary>
/// Typed HTTP client that wraps calls to the BookingCare doctors API.
/// </summary>
public sealed class DoctorApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DoctorApiClient> _logger;

    public DoctorApiClient(HttpClient httpClient, ILogger<DoctorApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetches the active doctor list from the backend API.
    /// </summary>
    public async Task<IReadOnlyList<DoctorListItem>> GetDoctorsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var doctors = await _httpClient.GetFromJsonAsync<List<DoctorListItem>>("api/admin/doctors", cancellationToken);
            return doctors ?? new List<DoctorListItem>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to retrieve doctor list from {BaseAddress}", _httpClient.BaseAddress);
            throw;
        }
    }
}
