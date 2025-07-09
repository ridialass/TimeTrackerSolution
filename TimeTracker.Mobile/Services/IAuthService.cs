// SECURITE :
// Ne jamais logger ni persister le mot de passe utilisateur dans ce service.
// Toujours transmettre les identifiants via HTTPS et uniquement via POST (jamais URL).
// Seul le token JWT peut être stocké localement, pas le mot de passe.

using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;
using TimeTracker.Mobile.Models;

namespace TimeTracker.Mobile.Services;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(string username, string password);
    Task<Result<bool>> RegisterAsync(RegisterRequestDto dto);
    Task LogoutAsync();
    Task<bool> TryRestoreSessionAsync();
    ApplicationUserSession? CurrentUser { get; }
}
