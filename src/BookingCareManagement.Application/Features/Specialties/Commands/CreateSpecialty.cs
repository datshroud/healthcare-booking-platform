using System;
using MediatR;

namespace BookingCareManagement.Application.Features.Specialties.Commands;

public record CreateSpecialty(string Name, string? Slug) : IRequest<Guid>;

public class CreateSpecialtyHandler : IRequestHandler<CreateSpecialty, Guid>
{
    public Task<Guid> Handle(CreateSpecialty request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
