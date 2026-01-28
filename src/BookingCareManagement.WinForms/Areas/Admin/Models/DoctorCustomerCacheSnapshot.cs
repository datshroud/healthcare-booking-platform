using System;
using System.Collections.Generic;
using BookingCareManagement.WinForms.Areas.Admin.Services.Models;
using BookingCareManagement.WinForms.Shared.Models.Dtos;

namespace BookingCareManagement.WinForms.Areas.Admin.Models;

public sealed record DoctorCustomerCacheSnapshot(
    DateOnly From,
    DateOnly To,
    DateTime UpdatedAtUtc,
    List<DoctorAppointmentListItemDto> Appointments,
    AdminDashboardOverviewDto? Overview,
    int DoctorCount,
    int CustomerCount
);
