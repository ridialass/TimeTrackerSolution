using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services
{
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
            => Task.FromResult(_preferences.Get<string?>(key, null));

        public Task RemoveAsync(string key)
        {
            _preferences.Remove(key);
            return Task.CompletedTask;
        }
    }
}
