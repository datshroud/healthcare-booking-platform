 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Application.Features.Auth.Dtos
{
    public sealed record AuthResponse(
        string AccessToken,
        DateTime ExpiresAt,
        string RefreshToken
    );
}
