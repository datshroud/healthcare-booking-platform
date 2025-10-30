using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCareManagement.Domain.Aggregates.User
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public bool IsActive => RevokedAt == null && DateTime.UtcNow <= ExpiresAt;
    }

    public static class RefreshTokenFactory
    {
        public static RefreshToken Create()
        {
            return new() { Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) };
        }
    }
}
