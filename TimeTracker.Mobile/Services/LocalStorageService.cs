#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Wrapper Preferences non-sensible (JSON, flags, etc.).
    /// Corrige l'appel à Preferences.Get<T>(key, default) en évitant un default null pour T=string.
    /// </summary>
    public sealed class LocalStorageService : ILocalStorageService
    {
        private readonly IPreferences _preferences;

        public LocalStorageService(IPreferences preferences) => _preferences = preferences;

        public Task SetStringAsync(string key, string value)
        {
            _preferences.Set(key, value);
            return Task.CompletedTask;
        }

        public Task<string?> GetStringAsync(string key)
        {
            if (!_preferences.ContainsKey(key))
                return Task.FromResult<string?>(null);

            var value = _preferences.Get(key, string.Empty); // default must be non-null
            return Task.FromResult<string?>(value);
        }

        public Task RemoveAsync(string key)
        {
            _preferences.Remove(key);
            return Task.CompletedTask;
        }
    }
}