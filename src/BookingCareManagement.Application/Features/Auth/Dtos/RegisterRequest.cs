using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Auth.Dtos
{
    public sealed record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string PhoneNumber,
        DateTime? DateOfBirth
    );
}
