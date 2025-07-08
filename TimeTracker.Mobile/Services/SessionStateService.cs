using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public class SessionStateService : ISessionStateService
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigation;
        private readonly ISecureStorageService _secureStorage;
        private const string InProgressSessionKey = "InProgressSession";

        public SessionStateService(
            IAuthService authService,
            INavigationService navigation,
            ISecureStorageService secureStorage)
        {
            _authService = authService;
            _navigation = navigation;
            _secureStorage = secureStorage;
        }

        public string? CurrentUserRole => _authService.CurrentUser?.Role;
        public object? CurrentUser => _authService.CurrentUser;

        public async Task<bool> TryRestoreSessionAsync()
        {
            // 1. Si pas de session, on reste sur LoginPage (ne navigue pas si déjà sur LoginPage)
            if (!await _authService.TryRestoreSessionAsync())
            {
                await _navigation.GoToLoginPageAsync();
                if (Shell.Current is AppShell appShell)
                    appShell.FlyoutBehavior = FlyoutBehavior.Disabled;
                return false;
            }

            // 2. Si session restaurée, on modifie le menu puis navigue selon le rôle
            if (Shell.Current is AppShell shell)
            {
                shell.FlyoutBehavior = FlyoutBehavior.Flyout;
                shell.ConfigureFlyoutForRole(CurrentUserRole ?? string.Empty);
            }

            if (CurrentUserRole == UserRole.Admin.ToString())
                await _navigation.GoToAdminDashboardPageAsync();
            else
                await _navigation.GoToHomePageAsync();

            return true;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var result = await _authService.LoginAsync(username, password);
            if (!result.IsSuccess)
                return false;

            if (Shell.Current is AppShell shell)
            {
                shell.ConfigureFlyoutForRole(CurrentUserRole ?? string.Empty);
                shell.FlyoutBehavior = FlyoutBehavior.Flyout;
            }

            if (CurrentUserRole == UserRole.Admin.ToString())
                await _navigation.GoToAdminDashboardPageAsync();
            else
                await _navigation.GoToHomePageAsync();

            return true;
        }

        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();

            // Diagnostic avancé
            System.Diagnostics.Debug.WriteLine($"[Logout] Shell.Current = {Shell.Current?.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[Logout] MainPage = {Application.Current?.MainPage?.GetType().FullName}");

            // Étape 1 : S'assurer que le Shell est bien la MainPage
            if (Application.Current.MainPage is not AppShell)
            {
                var newShell = TimeTracker.Mobile.App.ServiceProvider?.GetService<AppShell>() ?? new AppShell();
                Application.Current.MainPage = newShell;
                await Task.Delay(100); // Laisse le temps à la MainPage d'être affichée
            }

            // Étape 2 : S'assurer que le menu contient bien LoginPage SEUL (ResetForLogoutAsync)
            if (Application.Current.MainPage is AppShell shell)
            {
                await Task.Delay(50); // Synchronisation asynchrone du Shell
                await shell.ResetForLogoutAsync();
            }
            else
            {
                // Fallback ultime
                System.Diagnostics.Debug.WriteLine("[Logout] Impossible de retrouver un Shell MAUI valide après réinitialisation.");
            }

            System.Diagnostics.Debug.WriteLine("User logged out and shell reset.");
        }

        public async Task<TimeEntryDto?> GetCurrentSessionAsync()
        {
            var json = await _secureStorage.GetAsync(InProgressSessionKey);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonSerializer.Deserialize<TimeEntryDto>(json);
        }

        public async Task SetCurrentSessionAsync(TimeEntryDto session)
        {
            var json = JsonSerializer.Serialize(session);
            await _secureStorage.SetAsync(InProgressSessionKey, json);
        }

        public async Task ClearSessionAsync()
        {
            await _secureStorage.RemoveAsync(InProgressSessionKey);
        }
    }
}