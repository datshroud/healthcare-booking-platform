using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class AdminInvoiceApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminInvoiceApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync("/api/Invoice", cancellationToken);
        await EnsureSuccessAsync(response);
        var dtos = await response.Content.ReadFromJsonAsync<List<InvoiceDto>>(cancellationToken: cancellationToken);
        return dtos ?? new List<InvoiceDto>();
    }

    public async Task<InvoiceDto> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/Invoice/{invoiceId}", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: cancellationToken))!;
    }

    public async Task MarkAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.PostAsync($"/api/Invoice/{invoiceId}/mark-as-paid", null, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<byte[]> GetPdfAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        using var response = await client.GetAsync($"/api/Invoice/{invoiceId}/pdf", cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("BookingCareApi");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"API l?i {(int)response.StatusCode}: {detail}");
    }
}
