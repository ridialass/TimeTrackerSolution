using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Town { get; set; }
        public string? Country { get; set; }
    }
}

