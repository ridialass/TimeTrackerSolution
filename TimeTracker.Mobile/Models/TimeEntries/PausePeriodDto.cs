using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Models.TimeEntries
{
    public class PausePeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}
