using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public string? Username { get; set; } = default!; // Ajout
        public int UserId { get; set; } // Ajout
    }
}
