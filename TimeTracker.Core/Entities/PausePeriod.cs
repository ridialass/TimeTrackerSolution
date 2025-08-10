using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.Entities
{
    public class PausePeriod
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        // FK
        public int TimeEntryId { get; set; }
        public TimeEntry TimeEntry { get; set; } = default!;
    }
}
