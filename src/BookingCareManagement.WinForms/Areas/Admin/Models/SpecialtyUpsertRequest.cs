using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingCareManagement.WinForms.Areas.Admin.Models;

public sealed class SpecialtyUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Color { get; set; } = "#1a73e8";
    public decimal Price { get; set; }
    public IReadOnlyCollection<Guid> DoctorIds { get; set; } = Array.Empty<Guid>();

    public SpecialtyUpsertRequest Normalize()
    {
        Name = (Name ?? string.Empty).Trim();
        Slug = NormalizeOptional(Slug);
        Description = NormalizeOptional(Description);
        ImageUrl = NormalizeOptional(ImageUrl);
        Color = string.IsNullOrWhiteSpace(Color) ? "#1a73e8" : Color.Trim();
        DoctorIds = DoctorIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        return this;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
