using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Models.TimeEntries
{
    public class TimeEntryDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }            // [Required] côté serveur
        public DateTime? EndTime { get; set; }             // doit être renseigné à l’envoi

        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string? StartAddress { get; set; }

        public double? EndLatitude { get; set; }
        public double? EndLongitude { get; set; }
        public string? EndAddress { get; set; }

        public WorkSessionType SessionType { get; set; }   // [Required] côté serveur
        public bool IsAdminModified { get; set; }          // false côté mobile
        public bool IncludesTravelTime { get; set; }
        public double? TravelDurationHours { get; set; }

        public DinnerPaidBy DinnerPaid { get; set; }
        public string? Location { get; set; }

        public List<PausePeriodDto> Pauses { get; set; } = new();

        public int UserId { get; set; }                    // [Required]
        public string Username { get; set; } = string.Empty; // [Required]
    }
}
