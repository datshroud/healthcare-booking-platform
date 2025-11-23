using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Areas.Doctor.Services.Models;

public sealed class DashboardMetricPointDto
{
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}

public sealed class DashboardAppointmentTrendResponse
{
    public int ConfirmedCount { get; init; }
    public int CanceledCount { get; init; }
    public IReadOnlyList<DashboardMetricPointDto> Points { get; init; } = Array.Empty<DashboardMetricPointDto>();
    public string RangeLabel { get; init; } = string.Empty;
}

public sealed class DashboardSparklineResponse
{
    public decimal Total { get; init; }
    public string RangeLabel { get; init; } = string.Empty;
    public IReadOnlyList<DashboardMetricPointDto> Points { get; init; } = Array.Empty<DashboardMetricPointDto>();
}

public sealed class DashboardCustomerMixResponse
{
    public int NewCustomers { get; init; }
    public int ReturningCustomers { get; init; }
    public string RangeLabel { get; init; } = string.Empty;
}
