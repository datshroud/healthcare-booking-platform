using System;
using System.Linq;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Dtos;

public static class SpecialtyMappingExtensions
{
    public static SpecialtyDto ToDto(this Specialty specialty)
    {
        if (specialty is null)
        {
            throw new ArgumentNullException(nameof(specialty));
        }

        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name,
            Slug = specialty.Slug,
            Active = specialty.Active,
            Description = specialty.Description,
            ImageUrl = specialty.ImageUrl,
            Color = specialty.Color,
            Price = specialty.Price,
            Doctors = specialty.Doctors.Select(ToDoctorDto).ToList()
        };
    }

    public static SpecialtyDoctorDto ToDoctorDto(this Doctor doctor)
    {
        if (doctor is null)
        {
            throw new ArgumentNullException(nameof(doctor));
        }

        var parts = new[] { doctor.AppUser?.FirstName, doctor.AppUser?.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var fullName = parts.Length > 0
            ? string.Join(' ', parts)
            : doctor.AppUser?.UserName ?? "Bác sĩ";

        return new SpecialtyDoctorDto
        {
            Id = doctor.Id,
            FullName = fullName,
            AvatarUrl = doctor.AppUser?.AvatarUrl ?? string.Empty
        };
    }
}
