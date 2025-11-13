using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;
using BookingCareManagement.Domain.Aggregates.Doctor;

namespace BookingCareManagement.Application.Features.Specialties.Queries;

// 1. Query
public class GetAllSpecialtiesQuery { }

// 2. Handler
public class GetAllSpecialtiesQueryHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;

    public GetAllSpecialtiesQueryHandler(ISpecialtyRepository specialtyRepository)
    {
        _specialtyRepository = specialtyRepository;
    }

    public async Task<IEnumerable<SpecialtyDto>> Handle(CancellationToken cancellationToken)
    {
        var specialties = await _specialtyRepository.GetAllAsync(cancellationToken);

        // Map sang DTO
        return specialties.Select(s => new SpecialtyDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            Active = s.Active,
            Description = s.Description,
            ImageUrl = s.ImageUrl
        });
    }
}