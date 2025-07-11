using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class Verify2FACodeRequestDto
    {
        public string Username { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
