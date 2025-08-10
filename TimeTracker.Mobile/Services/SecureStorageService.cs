// TimeTracker.Mobile/Services/SecureStorageService.cs
#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Wrapper testable autour de SecureStorage (clé/valeur chiffré).
    /// À utiliser pour de petites données sensibles (ex: JWT).
    /// </summary>
    public sealed class SecureStorageService : ISecureStorageService
    {
        public async Task SetAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("La clé ne peut pas être vide.", nameof(key));

            try
            {
                await SecureStorage.Default.SetAsync(key, value ?? string.Empty);
            }
            catch (Exception)
            {
                // Laisse l'appelant gérer (log/fallback)
                throw;
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                return await SecureStorage.Default.GetAsync(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            try { SecureStorage.Default.Remove(key); }
            catch (Exception) { /* silencieux */ }
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
