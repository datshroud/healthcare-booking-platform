using System;
using System.Net.Http;
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
            // TODO: call /api/doctor/appointments/summary when backend is ready
            await Task.Delay(250, cancellationToken);
            _viewModel.TodayAppointments = Random.Shared.Next(0, 12);
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }
}
