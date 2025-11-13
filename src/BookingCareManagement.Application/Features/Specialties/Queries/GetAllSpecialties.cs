using BookingCareManagement.Application.Features.Specialties.Dtos;
using BookingCareManagement.Domain.Abstractions;

namespace BookingCareManagement.Application.Features.Specialties.Queries;

public class GetAllSpecialtiesQuery { }

public class GetAllSpecialtiesQueryHandler
{
    private readonly ISpecialtyRepository _specialtyRepository;

    public GetAllSpecialtiesQueryHandler(ISpecialtyRepository specialtyRepository)
    {
        _specialtyRepository = specialtyRepository;
    }

    public async Task<IEnumerable<SpecialtyDto>> Handle(GetAllSpecialtiesQuery query, CancellationToken cancellationToken)
    {
        var specialties = await _specialtyRepository.GetAllAsync(cancellationToken);

        return specialties.Select(s => s.ToDto());
    }
}
