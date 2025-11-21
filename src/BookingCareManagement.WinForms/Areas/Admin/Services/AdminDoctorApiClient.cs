using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Admin.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;
using System.IO;
using System.Text.Json;

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
        var dtos = await response.Content.ReadFromJsonAsync<List<DoctorDto>>(cancellationToken: cancellationToken) ?? new List<DoctorDto>();

        foreach (var d in dtos)
        {
            d.AvatarUrl = ToAbsoluteUrl(client, d.AvatarUrl);
        }

        return dtos;
    }

    public async Task<DoctorDto> GetByIdAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/Doctor/{doctorId}", cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = (await response.Content.ReadFromJsonAsync<DoctorDto>(cancellationToken: cancellationToken))!;
        if (dto != null)
        {
            dto.AvatarUrl = ToAbsoluteUrl(client, dto.AvatarUrl);
        }
        return dto;
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
            SpecialtyIds = request.SpecialtyIds,
            AvatarUrl = request.AvatarUrl
        };

        var client = CreateClient();
        using var response = await client.PostAsJsonAsync("/api/Doctor", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = (await response.Content.ReadFromJsonAsync<DoctorDto>(cancellationToken: cancellationToken))!;
        if (dto != null)
        {
            dto.AvatarUrl = ToAbsoluteUrl(client, dto.AvatarUrl);
        }
        return dto;
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
            SpecialtyIds = request.SpecialtyIds,
            AvatarUrl = request.AvatarUrl
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

    public async Task<string?> UploadFileAsync(string localFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(localFilePath) || !File.Exists(localFilePath)) return null;
        var client = CreateClient();

        using var content = new MultipartFormDataContent();
        await using var stream = File.OpenRead(localFilePath);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", Path.GetFileName(localFilePath));

        using var response = await client.PostAsync("/api/Upload", content, cancellationToken);
        await EnsureSuccessAsync(response);

        // try parse JSON object { avatarUrl: "/uploads/.." }
        var txt = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(txt)) return null;
        try
        {
            using var doc = JsonDocument.Parse(txt);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("avatarUrl", out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
        }
        catch
        {
            // ignore parse error
        }

        return txt;
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

    private static string ToAbsoluteUrl(HttpClient client, string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) return url!;

        try
        {
            var baseAddress = client.BaseAddress ?? new Uri("/");
            var absolute = new Uri(baseAddress, url).ToString();
            return absolute;
        }
        catch
        {
            return url!;
        }
    }
}
