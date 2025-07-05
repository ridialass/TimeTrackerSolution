using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public interface IMobileTimeEntryService
    {
        /// <summary>La session en cours, ou null s’il n’y en a pas.</summary>
        TimeEntryDto? InProgressSession { get; }

        /// <summary>Démarre une session en cours (stockée en mémoire).</summary>
        Task StartSessionAsync(TimeEntryDto dto);

        /// <summary>Termine et persiste la session en cours.</summary>
        Task EndAndSaveCurrentSessionAsync();
        /// <summary>Nouvelle méthode pour charger la session en cours depuis le stockage local.</summary> 
        Task LoadInProgressSessionAsync();

        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId);
        Task CreateTimeEntryAsync(TimeEntryDto entry);
    }


}
