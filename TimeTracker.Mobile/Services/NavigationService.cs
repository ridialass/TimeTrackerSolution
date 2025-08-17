#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        // Absolute roots (Shell root navigation): use TWO slashes.
        private const string RouteLoginRoot = "//LoginPage";
        private const string RouteHomeRoot = "//HomePage";
        private const string RouteAdminDashboardRoot = "//AdminDashboardPage";

        // Relative (stack) routes
        private const string RouteStartSession = "StartSessionPage";
        private const string RouteEndSession = "EndSessionPage";
        private const string RouteTimeEntries = "TimeEntriesPage";

        // -------- Public API (unchanged names) --------
        public Task GoToLoginPageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteLoginRoot));
        public Task GoToHomePageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteHomeRoot));
        public Task GoToAdminDashboardPageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteAdminDashboardRoot));

        public Task GoToStartSessionPageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteStartSession));
        public Task GoToEndSessionPageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteEndSession));
        public Task GoToTimeEntriesPageAsync() => NavigateOnUIAsync(() => ShellGoToAsync(RouteTimeEntries));

        public Task GoBackAsync() => NavigateOnUIAsync(() => ShellGoToAsync(".."));

        // Optional generic helper if you want to use it from VMs later
        public Task GoToAsync(string route, IDictionary<string, object?>? parameters = null, bool absoluteRoot = false)
        {
            var final = absoluteRoot && !route.StartsWith("//", StringComparison.Ordinal)
                ? $"//{route.TrimStart('/')}"
                : route;

            return NavigateOnUIAsync(() => ShellGoToAsync(final, parameters));
        }

        // ----------------- Helpers -----------------
        private static Task NavigateOnUIAsync(Func<Task> navigation)
        {
            var shell = Shell.Current;
            if (shell is null) return Task.CompletedTask; // UI not ready — no-op

            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await navigation().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[NavigationService] Navigation error: {ex}");
#endif
                    // swallow or rethrow depending on your policy
                }
            });
        }

        private static Task ShellGoToAsync(string route, IDictionary<string, object?>? parameters = null)
            => parameters is null
                ? Shell.Current.GoToAsync(route)
                : Shell.Current.GoToAsync(route, parameters);
    }
}
