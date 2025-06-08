using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.Entities
{
    public class Employee 
    {
        public int Id { get; set; }
        public string Username { get; set; }          // unique
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Town { get; set; }
        public string? Country { get; set; }

        // Navigation property (one‐to‐many)
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}
