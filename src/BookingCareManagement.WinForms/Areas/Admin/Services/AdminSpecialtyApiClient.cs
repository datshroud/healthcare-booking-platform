using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class AdminSpecialtyApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminSpecialtyApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<SpecialtyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync("/api/Specialty", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<SpecialtyDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<SpecialtyDto>();
    }

    public async Task<SpecialtyDto> CreateAsync(SpecialtyUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var payload = new
        {
            request.Name,
            request.Slug,
            request.Description,
            request.ImageUrl,
            Color = request.Color,
            DoctorIds = request.DoctorIds
        };

        var client = CreateClient();
        using var response = await client.PostAsJsonAsync("/api/Specialty", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SpecialtyDto>(cancellationToken: cancellationToken))!;
    }

    public async Task<SpecialtyDto> UpdateAsync(Guid specialtyId, SpecialtyUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var payload = new
        {
            request.Name,
            request.Slug,
            request.Description,
            request.ImageUrl,
            Color = request.Color,
            DoctorIds = request.DoctorIds
        };

        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/Specialty/{specialtyId}", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<SpecialtyDto>(cancellationToken: cancellationToken))!;
    }

    public async Task DeleteAsync(Guid specialtyId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.DeleteAsync($"/api/Specialty/{specialtyId}", cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("BookingCareApi");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"API lỗi {(int)response.StatusCode}: {detail}");
    }
}
