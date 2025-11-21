using System.ComponentModel.DataAnnotations;

namespace BookingCareManagement.Application.Features.Auth.Dtos
{
    public sealed class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
