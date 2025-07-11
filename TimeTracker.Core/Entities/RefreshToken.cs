using System;

namespace TimeTracker.Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;

        // Relation avec ApplicationUser
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;
    }
}