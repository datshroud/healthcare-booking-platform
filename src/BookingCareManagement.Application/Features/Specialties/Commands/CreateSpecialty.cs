using System;
using System.Collections.Generic;
using System.Linq;
using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public class CreateSpecialtyCommand
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public IEnumerable<Guid> DoctorIds { get; set; } = new List<Guid>();
}

public class CreateSpecialtyCommandHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSpecialtyCommandHandler(
        ISpecialtyRepository specialtyRepository,
        IDoctorRepository doctorRepository,
        IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _doctorRepository = doctorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SpecialtyDto> Handle(CreateSpecialtyCommand command, CancellationToken cancellationToken)
    {
        var specialty = new Specialty(
            command.Name,
            command.Slug,
            command.Description,
            command.ImageUrl,
            command.Color,
            NormalizePrice(command.Price)
        );

        _specialtyRepository.Add(specialty);

        if (command.DoctorIds?.Any() == true)
        {
            var doctors = await _doctorRepository.GetByIdsWithTrackingAsync(command.DoctorIds, cancellationToken);
            specialty.ReplaceDoctors(doctors);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return specialty.ToDto();
    }

    private static decimal NormalizePrice(decimal? price)
    {
        if (!price.HasValue)
        {
            return 0m;
        }

        if (price.Value < 0)
        {
            throw new ValidationException("Giá chuyên khoa không được âm.");
        }

        return decimal.Round(price.Value, 0, MidpointRounding.AwayFromZero);
    }
}
