using System;

namespace BookingCareManagement.Web.Areas.Customer.Dtos;

public sealed record CustomerDoctorSummaryDto(Guid Id, string FullName, string AvatarUrl);
