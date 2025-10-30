using System;

namespace BookingCareManagement.Infrastructure.Identity;

public sealed class GoogleOAuthSettings
{
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;

}
