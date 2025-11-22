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
        var dtos = await response.Content.ReadFromJsonAsync<List<SpecialtyDto>>(cancellationToken: cancellationToken) ?? new List<SpecialtyDto>();

        foreach (var s in dtos)
        {
            s.ImageUrl = ToAbsoluteUrl(client, s.ImageUrl);
            if (s.Doctors != null)
            {
                foreach (var d in s.Doctors)
                {
                    d.AvatarUrl = ToAbsoluteUrl(client, d.AvatarUrl);
                }
            }
        }

        return dtos;
    }

    public async Task<IReadOnlyList<CustomerSpecialtyDto>> GetCustomerAllAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync("/api/customer-booking/specialties", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<CustomerSpecialtyDto>>(cancellationToken: cancellationToken) ?? new List<CustomerSpecialtyDto>();

        foreach (var s in dtos)
        {
            s.ImageUrl = ToAbsoluteUrl(client, s.ImageUrl);
            if (s.Doctors != null)
            {
                foreach (var d in s.Doctors)
                {
                    d.AvatarUrl = ToAbsoluteUrl(client, d.AvatarUrl);
                }
            }
        }

        return dtos;
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
            Price = request.Price,
            DoctorIds = request.DoctorIds
        };

        var client = CreateClient();
        using var response = await client.PostAsJsonAsync("/api/Specialty", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = (await response.Content.ReadFromJsonAsync<SpecialtyDto>(cancellationToken: cancellationToken))!;
        if (dto != null)
        {
            dto.ImageUrl = ToAbsoluteUrl(client, dto.ImageUrl);
            if (dto.Doctors != null)
            {
                foreach (var d in dto.Doctors)
                {
                    d.AvatarUrl = ToAbsoluteUrl(client, d.AvatarUrl);
                }
            }
        }
        return dto;
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
            Price = request.Price,
            DoctorIds = request.DoctorIds
        };

        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/Specialty/{specialtyId}", payload, cancellationToken);
        await EnsureSuccessAsync(response);
        var dto = (await response.Content.ReadFromJsonAsync<SpecialtyDto>(cancellationToken: cancellationToken))!;
        if (dto != null)
        {
            dto.ImageUrl = ToAbsoluteUrl(client, dto.ImageUrl);
            if (dto.Doctors != null)
            {
                foreach (var d in dto.Doctors)
                {
                    d.AvatarUrl = ToAbsoluteUrl(client, d.AvatarUrl);
                }
            }
        }
        return dto;
    }

    public async Task DeleteAsync(Guid specialtyId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.DeleteAsync($"/api/Specialty/{specialtyId}", cancellationToken);
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
