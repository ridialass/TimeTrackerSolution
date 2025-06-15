using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.DTOs
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public int EmployeeId { get; set; }
        public string? Username { get; set; }
        public UserRole Role { get; set; }
    }
}

