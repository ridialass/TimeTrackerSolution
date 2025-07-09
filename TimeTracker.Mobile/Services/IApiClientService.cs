// SECURITE :
// Ne jamais logger ni persister le mot de passe ou les identifiants utilisateur dans ce service.
// Ce client doit toujours utiliser HTTPS pour toute communication réseau, surtout en production.
// Seul le token JWT peut être stocké localement (via AuthService), pas le mot de passe.
// Limiter l’exposition des messages d’erreur serveur côté UI (ne jamais retourner une erreur brute du serveur à l’utilisateur final).

using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services;

public interface IApiClientService
{
    Task<Result<LoginResponseDto>> LoginAsync(string username, string password);
    Task<Result<bool>> RegisterAsync(RegisterRequestDto dto);
    Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync();
    Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId);
    Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry);
}