#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Service mobile pour gérer le cycle de vie d'une session de travail côté app (DTO only).
    /// - <see cref="InProgressSession"/> est en lecture seule : utilise <see cref="StartSessionAsync"/> pour la définir.
    /// - La persistance locale de la session en cours est gérée par <see cref="LoadInProgressSessionAsync"/>.
    /// </summary>
    public interface IMobileTimeEntryService
    {
        /// <summary>La session en cours, ou <c>null</c> s’il n’y en a pas.</summary>
        TimeEntryDto? InProgressSession { get; }

        /// <summary>Démarre une session (la stocke en mémoire et en local si nécessaire).</summary>
        Task StartSessionAsync(TimeEntryDto dto);

        /// <summary>
        /// Termine et persiste la session en cours (envoie la mise à jour au serveur, puis vide l'état local).
        /// </summary>
        Task EndAndSaveCurrentSessionAsync();

        /// <summary>Charge, si elle existe, la session en cours depuis le stockage local.</summary>
        Task LoadInProgressSessionAsync();

        /// <summary>Récupère les pointages de l’utilisateur.</summary>
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId);

        /// <summary>
        /// Crée un nouveau pointage côté API. 
        /// Si l’API renvoie l’objet créé, l’Id est propagé dans <paramref name="entry"/>.
        /// </summary>
        Task CreateTimeEntryAsync(TimeEntryDto entry);
    }
}
