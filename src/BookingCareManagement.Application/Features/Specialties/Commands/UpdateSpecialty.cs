using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public sealed class UpdateSpecialtyCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public IEnumerable<Guid>? DoctorIds { get; set; }
}

public sealed class UpdateSpecialtyCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSpecialtyCommandHandler(
        ISpecialtyRepository specialtyRepository,
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpecialtyDto> Handle(UpdateSpecialtyCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ValidationException("Id chuyên khoa không hợp lệ.");
        }

        var trimmedName = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ValidationException("Tên chuyên khoa không được để trống.");
        }

        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Specialty with ID {command.Id} was not found.");

        var slug = BuildSlug(command.Slug, trimmedName);
        var description = NormalizeOptionalText(command.Description);
        var imageUrl = NormalizeOptionalText(command.ImageUrl);
        var color = NormalizeOptionalText(command.Color);

        specialty.Update(trimmedName, slug, description, imageUrl, color);

        var doctorIds = FilterDoctorIds(command.DoctorIds);
        if (doctorIds.Length > 0)
        {
            var doctors = await _doctorRepository.GetByIdsWithTrackingAsync(doctorIds, cancellationToken);
            EnsureDoctorsExist(doctorIds, doctors);
            specialty.ReplaceDoctors(doctors);
        }
        else
        {
            specialty.ReplaceDoctors(Array.Empty<Doctor>());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return specialty.ToDto();
    }

    private static Guid[] FilterDoctorIds(IEnumerable<Guid>? doctorIds)
    {
        return doctorIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();
    }

    private static void EnsureDoctorsExist(IEnumerable<Guid> requestedDoctorIds, IEnumerable<Doctor> foundDoctors)
    {
        var missingIds = requestedDoctorIds.Except(foundDoctors.Select(d => d.Id)).ToArray();
        if (missingIds.Length > 0)
        {
            throw new ValidationException($"Không thể gán bác sĩ: {string.Join(", ", missingIds)}.");
        }
    }

    private static string BuildSlug(string? slug, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? fallback : slug.Trim();
        return source
            .ToLowerInvariant()
            .Replace(' ', '-');
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
