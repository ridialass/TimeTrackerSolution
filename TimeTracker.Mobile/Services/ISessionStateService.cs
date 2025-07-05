using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services;

public interface ISessionStateService
{
    Task<TimeEntryDto?> GetCurrentSessionAsync();
    Task SetCurrentSessionAsync(TimeEntryDto? session);
    Task ClearSessionAsync();
}

