// SECURITE :
// Ne jamais logger ni persister le mot de passe ou les identifiants utilisateur dans ce service.
// Ce client doit toujours utiliser HTTPS pour toute communication réseau, surtout en production.
// Seul le token JWT peut être stocké localement (via AuthService), pas le mot de passe.
// Limiter l’exposition des messages d’erreur serveur côté UI (ne jamais retourner une erreur brute du serveur à l’utilisateur final).

#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Abstraction client HTTP de l’API côté mobile (DTO-only).
    /// Les erreurs brutes serveur ne doivent jamais être affichées à l’utilisateur final.
    /// </summary>
    public interface IApiClientService
    {
        // ----- Auth -----

        /// <summary>
        /// Authentifie un utilisateur et renvoie le token JWT (dans <see cref="LoginResponseDto.Token"/>)
        /// ainsi que les infos de base (Id, Username, Role).
        /// </summary>
        Task<Result<LoginResponseDto>> LoginAsync(string username, string password);

        /// <summary>
        /// Inscrit un nouvel utilisateur (réservé aux administrateurs côté API).
        /// </summary>
        Task<Result<bool>> RegisterAsync(RegisterRequestDto dto);

        // ----- Users -----

        /// <summary>
        /// Récupère la liste des employés.
        /// </summary>
        Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync();

        // ----- Time Entries -----

        /// <summary>
        /// Récupère les pointages d’un utilisateur.
        /// </summary>
        Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId);

        /// <summary>
        /// Crée un pointage. Si l’API renvoie l’objet créé, l’Id est propagé dans <paramref name="entry"/>.
        /// </summary>
        Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry);

        /// <summary>
        /// Charge un pointage complet (pauses incluses) pour l’édition.
        /// </summary>
        Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(int id);

        /// <summary>
        /// Met à jour un pointage (y compris la resynchronisation de la collection des pauses côté API).
        /// </summary>
        Task<Result<bool>> UpdateTimeEntryAsync(TimeEntryDto entry);
    }
}
