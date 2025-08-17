#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // MainThread
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Orchestrates mobile session state:
    /// - Auth via IAuthService (token in SecureStorage).
    /// - In-progress time entry via IMobileTimeEntryService.
    /// - Local cleanup via ILocalStorageService.
    /// - Shell menu and routing via AppShell helpers.
    /// </summary>
    public sealed class SessionStateService : ISessionStateService
    {
        private const string InProgressKey = "InProgressSession"; // keep aligned with MobileTimeEntryService

        private readonly IAuthService _auth;
        private readonly IMobileTimeEntryService _time;
        private readonly ILocalStorageService _storage;
        private readonly INavigationService _nav;
        private readonly ILogger<SessionStateService>? _logger;

        public SessionStateService(
            IAuthService authService,
            IMobileTimeEntryService timeEntryService,
            ILocalStorageService storage,
            INavigationService navigation,
            ILogger<SessionStateService>? logger = null)
        {
            _auth = authService;
            _time = timeEntryService;
            _storage = storage;
            _nav = navigation;
            _logger = logger;
        }

        public string? CurrentUserRole => _auth.CurrentUser?.Role;
        public object? CurrentUser => _auth.CurrentUser;

        /// <summary>
        /// Called on app start: restore auth from SecureStorage, then rebuild Shell and load in-progress session.
        /// </summary>
        public async Task<bool> TryRestoreSessionAsync()
        {
            bool authOk = false;
            try
            {
                authOk = await _auth.TryRestoreSessionAsync().ConfigureAwait(false);

                if (!authOk)
                {
                    _logger?.LogInformation("No valid auth token found; resetting Shell to Login.");
                    await ResetShellToLoginAsync().ConfigureAwait(false);
                }
                else
                {
                    var role = _auth.CurrentUser?.Role ?? string.Empty;
                    _logger?.LogInformation("Auth restored. Role='{role}' — configuring flyout and navigating to root.", role);
                    await ConfigureShellForRoleAsync(role).ConfigureAwait(false);
                }

                // Always attempt to reload local in-progress session (lightweight & safe).
                await _time.LoadInProgressSessionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error while restoring session state.");
                // In case of error, ensure we are safely on Login
                await ResetShellToLoginAsync().ConfigureAwait(false);
                return false;
            }

            return authOk;
        }

        /// <summary>
        /// Full login: API auth, restore profile, rebuild Shell, and bring any local in-progress session back.
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // Your IAuthService.LoginAsync likely returns a result object with IsSuccess.
                // We keep your original shape to avoid breaking changes.
                var res = await _auth.LoginAsync(username, password).ConfigureAwait(false);
                if (res is null)
                {
                    _logger?.LogWarning("AuthService.LoginAsync returned null result.");
                    return false;
                }

                var successProperty = res.GetType().GetProperty("IsSuccess");
                var isSuccess = successProperty?.GetValue(res) as bool? ?? false;
                if (!isSuccess)
                {
                    _logger?.LogInformation("Login failed for user '{username}'.", username);
                    return false;
                }

                // Ensure the in-memory auth state is fully restored (CurrentUser, Role, etc.).
                var ok = await _auth.TryRestoreSessionAsync().ConfigureAwait(false);
                if (!ok)
                {
                    _logger?.LogWarning("Token persisted but TryRestoreSessionAsync() failed to rebuild auth state.");
                    return false;
                }

                // Configure Shell according to role and navigate to absolute root page.
                var role = _auth.CurrentUser?.Role ?? string.Empty;
                await ConfigureShellForRoleAsync(role).ConfigureAwait(false);

                // Optionally reload local in-progress session
                await _time.LoadInProgressSessionAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "LoginAsync exception for user '{username}'.", username);
                await ResetShellToLoginAsync().ConfigureAwait(false);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _auth.LogoutAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "AuthService.LogoutAsync threw an exception; continuing with local cleanup.");
            }

            try
            {
                await ClearSessionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "ClearSessionAsync threw an exception during logout.");
            }

            await ResetShellToLoginAsync().ConfigureAwait(false);
        }

        public Task<TimeEntryDto?> GetCurrentSessionAsync()
            => Task.FromResult(_time.InProgressSession);

        public async Task SetCurrentSessionAsync(TimeEntryDto session)
        {
            // Sets in-memory + persists locally (delegated to service).
            await _time.StartSessionAsync(session).ConfigureAwait(false);
        }

        public async Task ClearSessionAsync()
        {
            // Remove the stored in-progress session and refresh the service cache.
            await _storage.RemoveAsync(InProgressKey).ConfigureAwait(false);
            await _time.LoadInProgressSessionAsync().ConfigureAwait(false);
        }

        // --------------------------
        // UI / Shell helpers (safe)
        // --------------------------

        private static Task RunOnUIAsync(Func<Task> action)
        {
            return MainThread.InvokeOnMainThreadAsync(action);
        }

        private Task ConfigureShellForRoleAsync(string role)
        {
            return RunOnUIAsync(async () =>
            {
                if (App.Current?.MainPage is AppShell shell)
                {
                    await shell.ConfigureFlyoutForRoleAsync(role);
                }
                else
                {
                    // Fallback: navigate using NavigationService if Shell not yet realized.
                    if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                        await _nav.GoToAdminDashboardPageAsync();
                    else
                        await _nav.GoToHomePageAsync();
                }
            });
        }

        private Task ResetShellToLoginAsync()
        {
            return RunOnUIAsync(async () =>
            {
                if (App.Current?.MainPage is AppShell shell)
                {
                    await shell.ResetForLogoutAsync();
                }
                else
                {
                    await _nav.GoToLoginPageAsync();
                }
            });
        }
    }
}
