using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.Services;

public interface ISessionStateService
{
    Task<bool> TryRestoreSessionAsync();
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    string? CurrentUserRole { get; }
    object? CurrentUser { get; }

    Task<TimeEntryDto?> GetCurrentSessionAsync();
    Task SetCurrentSessionAsync(TimeEntryDto session);
    Task ClearSessionAsync();
}