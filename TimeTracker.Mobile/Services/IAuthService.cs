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