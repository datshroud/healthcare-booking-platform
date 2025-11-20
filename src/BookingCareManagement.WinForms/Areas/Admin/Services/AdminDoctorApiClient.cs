using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class AdminDoctorApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminDoctorApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync("/api/Doctor", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<DoctorDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<DoctorDto>();
    }

    public async Task<DoctorDto> GetByIdAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/Doctor/{doctorId}", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<DoctorDto>(cancellationToken: cancellationToken))!;
    }

    public async Task<DoctorWorkingHoursDto> GetWorkingHoursAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/doctor/{doctorId}/hours", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<DoctorWorkingHoursDto>(cancellationToken: cancellationToken))!;
    }

    public async Task<IReadOnlyList<DoctorDayOffDto>> GetDayOffsAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/doctor/{doctorId}/dayoffs", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<DoctorDayOffDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<DoctorDayOffDto>();
    }

    public async Task<DoctorDayOffDto?> CreateDayOffAsync(Guid doctorId, object dayOffRequest, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PostAsJsonAsync($"/api/doctor/{doctorId}/dayoffs", dayOffRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await EnsureSuccessAsync(response);
            return default;
        }
        return await response.Content.ReadFromJsonAsync<DoctorDayOffDto>(cancellationToken: cancellationToken);
    }

    public async Task UpdateDayOffAsync(Guid doctorId, Guid dayOffId, object dayOffRequest, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/doctor/{doctorId}/dayoffs/{dayOffId}", dayOffRequest, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteDayOffAsync(Guid doctorId, Guid dayOffId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.DeleteAsync($"/api/doctor/{doctorId}/dayoffs/{dayOffId}", cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<DoctorDto> CreateAsync(DoctorUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var payload = new
        {
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            SpecialtyIds = request.SpecialtyIds
        };

        var client = CreateClient();
        using var response = await client.PostAsJsonAsync("/api/Doctor", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<DoctorDto>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(Guid doctorId, DoctorUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request.Normalize();
        var payload = new
        {
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            SpecialtyIds = request.SpecialtyIds
        };

        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/Doctor/{doctorId}", payload, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    // profile / hours update
    public async Task UpdateProfileAsync(Guid doctorId, object profileRequest, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/doctor/{doctorId}/profile", profileRequest, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task UpdateWorkingHoursAsync(Guid doctorId, object hoursRequest, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/doctor/{doctorId}/hours", hoursRequest, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.DeleteAsync($"/api/Doctor/{doctorId}", cancellationToken);
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
