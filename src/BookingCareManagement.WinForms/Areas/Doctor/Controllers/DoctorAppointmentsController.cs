using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Doctor.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Doctor.Controllers;

public sealed class DoctorAppointmentsController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DoctorAppointmentsViewModel _viewModel;

    public DoctorAppointmentsController(IHttpClientFactory httpClientFactory, DoctorAppointmentsViewModel viewModel)
    {
        _httpClientFactory = httpClientFactory;
        _viewModel = viewModel;
    }

    public async Task RefreshDashboardAsync(CancellationToken cancellationToken = default)
    {
        _viewModel.IsBusy = true;
        try
        {
            var client = _httpClientFactory.CreateClient("BookingCareApi");
            var today = DateOnly.FromDateTime(DateTime.Now);
            var resp = await client.GetAsync($"api/doctor/appointments?from={today:yyyy-MM-dd}&to={today:yyyy-MM-dd}", cancellationToken);
            resp.EnsureSuccessStatusCode();
            var appointments = await resp.Content.ReadFromJsonAsync<System.Collections.Generic.List<BookingCareManagement.WinForms.Shared.Models.Dtos.DoctorAppointmentListItemDto>>(cancellationToken: cancellationToken);
            _viewModel.TodayAppointments = appointments?.Count ?? 0;
        }
        catch (Exception ex)
        {
            // Có thể log lỗi hoặc hiển thị thông báo
            _viewModel.TodayAppointments = 0;
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }
}
