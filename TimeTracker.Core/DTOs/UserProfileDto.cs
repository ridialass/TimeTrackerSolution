using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class UserProfileDto
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Town { get; set; }
        public required string Country { get; set; }
        public required string ProfilePictureUrl { get; set; }
    }
}
