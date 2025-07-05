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
            await SafeNavigateAsync("///LoginPage");
        }

        public async Task GoToHomePageAsync()
        {
            await SafeNavigateAsync("///HomePage");
        }

        public async Task GoToAdminDashboardPageAsync()
        {
            await SafeNavigateAsync("///AdminDashboardPage");
        }

        public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            await SafeNavigateAsync(route, parameters);
        }

        private static async Task SafeNavigateAsync(string route, IDictionary<string, object>? parameters = null)
        {
            try
            {
                // Pages accessibles en navigation absolue (ShellContent racine)
                var absoluteRoutes = new HashSet<string>
                {
                    "LoginPage",
                    "HomePage",
                    "AdminDashboardPage"
                    // Ajoute ici d'autres pages Shell racine si besoin
                };

                if (absoluteRoutes.Contains(route.TrimStart('/')))
                {
                    var absRoute = route.StartsWith("///") ? route : $"///{route.TrimStart('/')}";
                    if (parameters != null)
                        await Shell.Current.GoToAsync(absRoute, parameters);
                    else
                        await Shell.Current.GoToAsync(absRoute);
                }
                else
                {
                    // Navigation relative ou via route enregistrée
                    if (parameters != null)
                        await Shell.Current.GoToAsync(route, parameters);
                    else
                        await Shell.Current.GoToAsync(route);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to {route} failed: {ex.Message}");
            }
        }
    }
}