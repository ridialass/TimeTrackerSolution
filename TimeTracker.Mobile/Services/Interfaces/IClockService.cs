using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services.Interfaces
{
    public interface IClockService
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
