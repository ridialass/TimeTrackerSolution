using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.DTOs
{
    public class PausePeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}
