using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    public class SystemClockService : IClockService
    {
        public DateTime Now => DateTime.UtcNow; // UTC, pour éviter la triche
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
