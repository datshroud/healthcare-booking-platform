using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Admin.Services.Models;

namespace BookingCareManagement.WinForms.Areas.Admin.Services;

public sealed class AdminDashboardApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminDashboardApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AdminDashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("BookingCareApi");
        var dto = await client.GetFromJsonAsync<AdminDashboardOverviewDto>("api/admin/dashboard/overview", cancellationToken);
        return dto ?? new AdminDashboardOverviewDto();
    }
}
