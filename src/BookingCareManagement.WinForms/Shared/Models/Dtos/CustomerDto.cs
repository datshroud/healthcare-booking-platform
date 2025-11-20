using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.WinForms.Shared.Models.Dtos
{
    public sealed class CustomerDto
    {
        public string Id { get; set; } = string.Empty; // AppUser.Id
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? InternalNote { get; set; }
        public DateTime CreatedAt { get; set; }

        // Các trường này sẽ được tính toán
        public int AppointmentCount { get; set; }
        public DateTime? LastAppointment { get; set; }
    }
}
