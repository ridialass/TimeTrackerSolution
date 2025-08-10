#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        // Routes : adapter si tes routes Shell diffèrent
        private const string RouteLoginRoot = "//LoginPage";
        private const string RouteHomeRoot = "//HomePage";
        private const string RouteAdminDashboardRoot = "//AdminDashboardPage";

        private const string RouteStartSession = "StartSessionPage";
        private const string RouteEndSession = "EndSessionPage";
        private const string RouteTimeEntries = "TimeEntriesPage";

        public Task GoToLoginPageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteLoginRoot));
        public Task GoToHomePageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteHomeRoot));
        public Task GoToAdminDashboardPageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteAdminDashboardRoot));

        public Task GoToStartSessionPageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteStartSession));
        public Task GoToEndSessionPageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteEndSession));
        public Task GoToTimeEntriesPageAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(RouteTimeEntries));

        public Task GoBackAsync() => NavigateOnUIAsync(() => Shell.Current.GoToAsync(".."));

        // ----------------- Helpers -----------------

        private static Task NavigateOnUIAsync(Func<Task> navigation)
        {
            var shell = Shell.Current;
            if (shell is null) return Task.CompletedTask; // UI pas prête : on échoue silencieusement
            return MainThread.InvokeOnMainThreadAsync(navigation);
        }
    }
}
