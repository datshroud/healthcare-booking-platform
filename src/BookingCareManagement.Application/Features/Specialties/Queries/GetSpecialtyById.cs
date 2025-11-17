using BookingCareManagement.Application.Common.Exceptions;
using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Queries;

public class GetSpecialtyByIdQuery
{
    public Guid Id { get; set; }
}

public class GetSpecialtyByIdQueryHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;

    public GetSpecialtyByIdQueryHandler(ISpecialtyRepository specialtyRepository)
    {
        _specialtyRepository = specialtyRepository;
    }

    public async Task<SpecialtyDto> Handle(GetSpecialtyByIdQuery query, CancellationToken cancellationToken)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(query.Id, cancellationToken);
        if (specialty is null)
        {
            throw new NotFoundException($"Specialty with ID {query.Id} was not found.");
        }

        return specialty.ToDto();
    }
}
