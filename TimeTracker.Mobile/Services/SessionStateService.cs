// TimeTracker.Mobile/Services/SessionStateService.cs
#nullable enable
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Orchestrateur d'état de session côté mobile.
    /// - Délègue l'auth à <see cref="IAuthService"/>.
    /// - Délègue la gestion du pointage en cours à <see cref="IMobileTimeEntryService"/>.
    /// - Nettoie le stockage local si besoin via <see cref="ILocalStorageService"/>.
    /// </summary>
    public sealed class SessionStateService : ISessionStateService
    {
        private const string InProgressKey = "InProgressSession"; // doit rester aligné avec MobileTimeEntryService

        private readonly IAuthService _auth;
        private readonly IMobileTimeEntryService _time;
        private readonly ILocalStorageService _storage;

        public SessionStateService(
            IAuthService authService,
            IMobileTimeEntryService timeEntryService,
            ILocalStorageService storage)
        {
            _auth = authService;
            _time = timeEntryService;
            _storage = storage;
        }

        public string? CurrentUserRole => _auth.CurrentUser?.Role;
        public object? CurrentUser => _auth.CurrentUser;

        public async Task<bool> TryRestoreSessionAsync()
        {
            // 1) Restaurer l'auth (JWT en SecureStorage)
            var authOk = await _auth.TryRestoreSessionAsync();

            // 2) Restaurer la session de pointage en cours (depuis le stockage local non sensible)
            await _time.LoadInProgressSessionAsync();

            return authOk;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var res = await _auth.LoginAsync(username, password);
            if (!res.IsSuccess) return false;

            // Optionnel : tenter de recharger une session en cours si existante
            await _time.LoadInProgressSessionAsync();
            return true;
        }

        public async Task LogoutAsync()
        {
            await _auth.LogoutAsync();
            await ClearSessionAsync();
        }

        public Task<TimeEntryDto?> GetCurrentSessionAsync() =>
            Task.FromResult(_time.InProgressSession);

        public async Task SetCurrentSessionAsync(TimeEntryDto session)
        {
            // Définit la session en cours (en mémoire + persistance locale par le service)
            await _time.StartSessionAsync(session);
        }

        public async Task ClearSessionAsync()
        {
            // Supprime la session en cours du stockage local,
            // puis recharge l'état du service pour refléter l'effacement.
            await _storage.RemoveAsync(InProgressKey);
            await _time.LoadInProgressSessionAsync();
        }
    }
}
