using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingCareManagement.WinForms.Areas.Admin.Models;

public sealed class DoctorUpsertRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public IReadOnlyCollection<Guid> SpecialtyIds { get; set; } = Array.Empty<Guid>();

    public DoctorUpsertRequest Normalize()
    {
        FirstName = (FirstName ?? string.Empty).Trim();
        LastName = (LastName ?? string.Empty).Trim();
        Email = (Email ?? string.Empty).Trim();
        PhoneNumber = (PhoneNumber ?? string.Empty).Trim();
        AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim();
        SpecialtyIds = SpecialtyIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        return this;
    }
}
