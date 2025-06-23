using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        public async Task GoToLoginPageAsync()
        {
            await SafeNavigateAsync(nameof(LoginPage));
        }

        public async Task GoToHomePageAsync()
        {
            await SafeNavigateAsync(nameof(HomePage));
        }

        public async Task GoToAdminDashboardPageAsync()
        {
            await SafeNavigateAsync(nameof(AdminDashboardPage));
        }

        public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            await SafeNavigateAsync(route, parameters);
        }

        private static async Task SafeNavigateAsync(string route, IDictionary<string, object>? parameters = null)
        {
            try
            {
                // Pages nécessitant navigation relative (jamais absolue !)
                var relativeRoutes = new HashSet<string>
                {
                    nameof(LoginPage),
                    nameof(StartSessionPage),
                    nameof(EndSessionPage),
                    nameof(TimeEntriesPage)
                    // Ajoute ici d'autres pages modales/secondaires si besoin
                };

                if (relativeRoutes.Contains(route))
                {
                    // Navigation relative
                    if (parameters != null)
                        await Shell.Current.GoToAsync(route, parameters);
                    else
                        await Shell.Current.GoToAsync(route);
                }
                else
                {
                    // Seulement pour HomePage/AdminDashboardPage (Flyout) → navigation absolue
                    var absRoute = route.StartsWith("//") ? route : $"//{route}";
                    if (parameters != null)
                        await Shell.Current.GoToAsync(absRoute, parameters);
                    else
                        await Shell.Current.GoToAsync(absRoute);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to {route} failed: {ex.Message}");
            }
        }
    }
}