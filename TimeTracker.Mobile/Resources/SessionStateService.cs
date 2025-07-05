using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services;
public class SessionStateService : ISessionStateService
{
    private const string SessionKey = "CurrentSession";
    private TimeEntryDto? _cachedSession;

    public async Task<TimeEntryDto?> GetCurrentSessionAsync()
    {
        if (_cachedSession != null)
            return _cachedSession;

        if (Preferences.Default.ContainsKey(SessionKey))
        {
            var json = Preferences.Default.Get<string>(SessionKey, null);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _cachedSession = JsonSerializer.Deserialize<TimeEntryDto>(json);
                }
                catch { _cachedSession = null; }
            }
        }
        return _cachedSession;
    }

    public async Task SetCurrentSessionAsync(TimeEntryDto? session)
    {
        _cachedSession = session;
        if (session == null)
        {
            Preferences.Default.Remove(SessionKey);
        }
        else
        {
            var json = JsonSerializer.Serialize(session);
            Preferences.Default.Set(SessionKey, json);
        }
    }

    public async Task ClearSessionAsync()
    {
        _cachedSession = null;
        Preferences.Default.Remove(SessionKey);
    }
}
