using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services.Interfaces
{
    /// <summary>
    /// Simple, string-focused wrapper over MAUI Preferences.
    /// Avoids null-generic issues and keeps API clean.
    /// </summary>
    public interface IPreferencesService
    {
        string? GetString(string key);
        void SetString(string key, string? value);
        void Remove(string key);
    }
}
