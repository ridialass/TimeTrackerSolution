using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.Entities
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string? StartAddress { get; set; }

        public double? EndLatitude { get; set; }
        public double? EndLongitude { get; set; }
        public string? EndAddress { get; set; }

        public WorkSessionType SessionType { get; set; }
        public bool IncludesTravelTime { get; set; }
        public double? TravelDurationHours { get; set; }

        public DinnerPaidBy DinnerPaid { get; set; }
        public string? Location { get; set; }

        // FK to Employee
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public TimeSpan? WorkDuration =>
            EndTime.HasValue ? EndTime.Value - StartTime : null;

        public TimeSpan? TravelTimeEstimate =>
            (IncludesTravelTime && TravelDurationHours.HasValue)
                ? TimeSpan.FromHours(TravelDurationHours.Value)
                : null;
    }
}
