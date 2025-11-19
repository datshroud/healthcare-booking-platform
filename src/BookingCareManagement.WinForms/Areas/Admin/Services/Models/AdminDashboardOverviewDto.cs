using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Areas.Admin.Services.Models;

public sealed class AdminDashboardOverviewDto
{
    public List<AdminDashboardCardDto> Cards { get; init; } = new();
    public List<AdminDashboardAppointmentDto> UpcomingAppointments { get; init; } = new();
    public List<AdminDashboardActivityDto> Activities { get; init; } = new();
}

public sealed class AdminDashboardCardDto
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string TrendLabel { get; init; } = string.Empty;
    public string AccentColor { get; init; } = "#2563eb";
}

public sealed class AdminDashboardAppointmentDto
{
    public Guid Id { get; init; }
    public DateTime StartUtc { get; init; }
    public string TimeLabel { get; init; } = string.Empty;
    public string DoctorName { get; init; } = string.Empty;
    public string ServiceName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class AdminDashboardActivityDto
{
    public string Time { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
