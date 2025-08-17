#nullable enable
using System.Threading.Tasks;

namespace TimeTracker.Mobile.Services.Interfaces
{
    /// <summary>Stockage clé/valeur non sensible (Preferences).</summary>
    public interface ILocalStorageService
    {
        Task SetStringAsync(string key, string value);
        Task<string?> GetStringAsync(string key);
        Task RemoveAsync(string key);
    }
}
