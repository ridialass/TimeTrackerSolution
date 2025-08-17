using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Maps empty string to null, because Preferences requires non-null defaults.
    /// </summary>
    public sealed class PreferencesService : IPreferencesService
    {
        public string? GetString(string key)
        {
            var value = Preferences.Get(key, string.Empty);
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public void SetString(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
                Preferences.Remove(key);
            else
                Preferences.Set(key, value);
        }

        public void Remove(string key) => Preferences.Remove(key);
    }
}
