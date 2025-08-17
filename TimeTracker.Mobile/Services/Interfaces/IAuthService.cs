using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;
using TimeTracker.Mobile.Models;

namespace TimeTracker.Mobile.Services.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> TryRestoreSessionAsync();
    ApplicationUserSession? CurrentUser { get; }
}