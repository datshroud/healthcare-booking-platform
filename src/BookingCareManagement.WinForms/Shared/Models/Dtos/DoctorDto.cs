using System;
using System.Collections.Generic;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos;

public sealed record DoctorDto(Guid Id, string FullName, IReadOnlyCollection<string> Specialties);
