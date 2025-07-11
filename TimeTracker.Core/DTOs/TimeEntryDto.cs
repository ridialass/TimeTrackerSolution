using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.DTOs
{
    public class TimeEntryDto
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
        public bool IsAdminModified { get; set; }
        public bool IncludesTravelTime { get; set; }
        public double? TravelDurationHours { get; set; }

        public DinnerPaidBy DinnerPaid { get; set; }
        public string? Location { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; } = default!;

        // ── PROPRIÉTÉS CALCULÉES ───────────────────────────────────

        /// <summary>
        /// Correspond au champ calculé dans l’entité EF : s’il y a une EndTime,
        /// retourne EndTime – StartTime, sinon null.
        /// </summary>
        public TimeSpan? WorkDuration =>
            EndTime.HasValue
                ? EndTime.Value - StartTime
                : null;

        /// <summary>
        /// Si IncludesTravelTime est vrai et que TravelDurationHours a une valeur,
        /// retourne un TimeSpan à partir de ce nombre d’heures.
        /// </summary>
        public TimeSpan? TravelTimeEstimate
        {
            get
            {
                if (IncludesTravelTime && TravelDurationHours.HasValue)
                    return TimeSpan.FromHours(TravelDurationHours.Value);
                return null;
            }
        }
    }
}

