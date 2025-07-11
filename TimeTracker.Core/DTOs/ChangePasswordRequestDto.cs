using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class ChangePasswordRequestDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}