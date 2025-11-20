using System;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.WinForms.Areas.Doctor.Services;
using BookingCareManagement.WinForms.Areas.Doctor.ViewModels;

namespace BookingCareManagement.WinForms.Areas.Doctor.Controllers;

public sealed class DoctorAppointmentsController
{
    private readonly DoctorAppointmentsApiClient _apiClient;
    private readonly DoctorAppointmentsViewModel _viewModel;

    public DoctorAppointmentsController(DoctorAppointmentsApiClient apiClient, DoctorAppointmentsViewModel viewModel)
    {
        _apiClient = apiClient;
        _viewModel = viewModel;
    }

    public async Task RefreshDashboardAsync(CancellationToken cancellationToken = default)
    {
        _viewModel.IsBusy = true;
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var appointments = await _apiClient.GetAppointmentsAsync(today, today, cancellationToken);
            _viewModel.TodayAppointments = appointments.Count;
        }
        catch (Exception)
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
