using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Models
{

    /// <summary>
    /// Lightweight user session for mobile authentication.
    /// </summary>
    public class ApplicationUserSession
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
    }
}

