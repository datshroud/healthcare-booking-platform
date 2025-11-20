using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class CustomerService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CustomerService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Lấy danh sách khách hàng
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync("/api/customer", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<CustomerDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<CustomerDto>();
    }

    // Lấy khách hàng theo ID
    public async Task<CustomerDto> GetByIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/customer/{customerId}", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken: cancellationToken))!;
    }

    // Tìm kiếm khách hàng
    public async Task<IReadOnlyList<CustomerDto>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();

        // Some server implementations expect POST for search; prefer POST to avoid405.
        var payload = new { keyword };
        using var response = await client.PostAsJsonAsync("/api/customer/search", payload, cancellationToken);

        // If server rejects POST with405, try GET as fallback.
        if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
        {
            response.Dispose();
            using var getResp = await client.GetAsync($"/api/customer/search?keyword={System.Uri.EscapeDataString(keyword)}", cancellationToken);
            await EnsureSuccessAsync(getResp);
            var dtosGet = await getResp.Content.ReadFromJsonAsync<List<CustomerDto>>(cancellationToken: cancellationToken);
            return dtosGet ?? new List<CustomerDto>();
        }

        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<CustomerDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<CustomerDto>();
    }

    // Thêm khách hàng mới
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PostAsJsonAsync("/api/customer", request, cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken: cancellationToken))!;
    }

    // Cập nhật khách hàng
    public async Task UpdateAsync(string customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/customer/{customerId}", request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    // Xóa khách hàng
    public async Task DeleteAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.DeleteAsync($"/api/customer/{customerId}", cancellationToken);
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

// Request model để tạo khách hàng mới
public class CreateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? InternalNote { get; set; }
    public bool SendWelcomeEmail { get; set; }
}

// Request model để cập nhật khách hàng
public class UpdateCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? InternalNote { get; set; }
}