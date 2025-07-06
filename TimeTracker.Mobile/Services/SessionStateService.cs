using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public class SessionStateService : ISessionStateService
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigation;
        private readonly AppShell _shell;
        private readonly ISecureStorageService _secureStorage;
        private const string InProgressSessionKey = "InProgressSession";

        public SessionStateService(
            IAuthService authService,
            INavigationService navigation,
            AppShell shell,
            ISecureStorageService secureStorage)
        {
            _authService = authService;
            _navigation = navigation;
            _shell = shell;
            _secureStorage = secureStorage;
        }

        public string? CurrentUserRole => _authService.CurrentUser?.Role;
        public object? CurrentUser => _authService.CurrentUser;

        public async Task<bool> TryRestoreSessionAsync()
        {
            if (!await _authService.TryRestoreSessionAsync())
            {
                await _navigation.GoToLoginPageAsync();
                _shell.FlyoutBehavior = FlyoutBehavior.Disabled;
                return false;
            }

            _shell.FlyoutBehavior = FlyoutBehavior.Flyout;
            _shell.ConfigureFlyoutForRole(CurrentUserRole ?? string.Empty);

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

            _shell.ConfigureFlyoutForRole(CurrentUserRole ?? string.Empty);
            _shell.FlyoutBehavior = FlyoutBehavior.Flyout;

            if (CurrentUserRole == UserRole.Admin.ToString())
                await _navigation.GoToAdminDashboardPageAsync();
            else
                await _navigation.GoToHomePageAsync();

            return true;
        }

        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            await _shell.ResetForLogoutAsync();
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
