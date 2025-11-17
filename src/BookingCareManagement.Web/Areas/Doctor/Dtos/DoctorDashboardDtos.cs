using System;
using System.Collections.Generic;

namespace BookingCareManagement.Web.Areas.Doctor.Dtos;

public sealed class DashboardMetricPointDto
{
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}

public sealed class DashboardSparklineResponse
{
    public decimal Total { get; init; }
    public string RangeLabel { get; init; } = string.Empty;
    public IReadOnlyList<DashboardMetricPointDto> Points { get; init; } = Array.Empty<DashboardMetricPointDto>();
}

public sealed class DashboardAppointmentTrendResponse
{
    public int ConfirmedCount { get; init; }
    public int CanceledCount { get; init; }
    public IReadOnlyList<DashboardMetricPointDto> Points { get; init; } = Array.Empty<DashboardMetricPointDto>();
    public string RangeLabel { get; init; } = string.Empty;
}

public sealed class DashboardCustomerMixResponse
{
    public int NewCustomers { get; init; }
    public int ReturningCustomers { get; init; }
    public string RangeLabel { get; init; } = string.Empty;
}

public sealed class DashboardHeatmapCellDto
{
    public DateOnly Date { get; init; }
    public double OccupancyPercent { get; init; }
}

public sealed class DashboardHeatmapResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public IReadOnlyList<DashboardHeatmapCellDto> Cells { get; init; } = Array.Empty<DashboardHeatmapCellDto>();
}
