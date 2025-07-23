using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.Core.DTOs
{
    public class TimeEntryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "StartTime est obligatoire.")]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string? StartAddress { get; set; }

        public double? EndLatitude { get; set; }
        public double? EndLongitude { get; set; }
        public string? EndAddress { get; set; }

        [Required(ErrorMessage = "SessionType est obligatoire.")]
        public WorkSessionType SessionType { get; set; }

        public bool IsAdminModified { get; set; }
        public bool IncludesTravelTime { get; set; }
        public double? TravelDurationHours { get; set; }

        public DinnerPaidBy DinnerPaid { get; set; }
        public string? Location { get; set; }

        // Historique des pauses multiples
        public List<PausePeriod> Pauses { get; set; } = new();

        [Required(ErrorMessage = "UserId est obligatoire.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId doit être supérieur à 0.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username est obligatoire.")]
        public string Username { get; set; } = default!;

        // ── PROPRIÉTÉS CALCULÉES ───────────────────────────────────

        /// <summary>
        /// Durée de travail brut : EndTime – StartTime
        /// </summary>
        [JsonIgnore]
        public TimeSpan? WorkDuration =>
            EndTime.HasValue ? EndTime.Value - StartTime : null;

        /// <summary>
        /// Durée cumulée de toutes les pauses
        /// </summary>
        [JsonIgnore]
        public TimeSpan TotalPauseDuration =>
            TimeSpan.FromSeconds(
                Pauses
                    .Where(p => p.End.HasValue)
                    .Sum(p => (p.End.Value - p.Start).TotalSeconds)
            );

        /// <summary>
        /// Estimation de temps de déplacement
        /// </summary>
        [JsonIgnore]
        public TimeSpan? TravelTimeEstimate =>
            IncludesTravelTime && TravelDurationHours.HasValue
                ? TimeSpan.FromHours(TravelDurationHours.Value)
                : null;
    }
}
