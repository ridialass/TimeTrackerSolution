using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = default!;
    }
}
