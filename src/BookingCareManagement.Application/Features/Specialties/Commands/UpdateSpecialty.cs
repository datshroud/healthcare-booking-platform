using System;
using System.Collections.Generic;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public class UpdateSpecialtyCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public IEnumerable<Guid> DoctorIds { get; set; } = new List<Guid>();
}

public class UpdateSpecialtyCommandHandler
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
        var specialty = await _specialtyRepository.GetByIdWithTrackingAsync(command.Id, cancellationToken);
        if (specialty == null)
        {
            throw new NotFoundException($"Specialty with ID {command.Id} was not found.");
        }

        specialty.Update(
            command.Name,
            command.Slug,
            command.Description,
            command.ImageUrl,
            command.Color
        );

        var doctorIds = command.DoctorIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray() ?? Array.Empty<Guid>();
        if (doctorIds.Length > 0)
        {
            var doctors = await _doctorRepository.GetByIdsWithTrackingAsync(doctorIds, cancellationToken);
            specialty.ReplaceDoctors(doctors);
        }
        else
        {
            specialty.ReplaceDoctors(Array.Empty<Doctor>());
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return specialty.ToDto();
    }
}
