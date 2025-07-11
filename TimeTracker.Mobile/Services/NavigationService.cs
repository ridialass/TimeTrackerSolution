using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToLoginPageAsync()
        {
            if (Shell.Current is Shell shell)
            {
                var loginFlyoutItem = shell.Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "LoginPage");
                if (loginFlyoutItem != null)
                {
                    if (shell.CurrentItem == loginFlyoutItem)
                        return Task.CompletedTask;
                    shell.CurrentItem = loginFlyoutItem;
                    return Task.CompletedTask;
                }
                throw new System.Exception("Aucune racine LoginPage disponible dans le Shell !");
            }
            throw new System.Exception("Shell.Current n'est pas un Shell valide.");
        }

        public Task GoToHomePageAsync()
        {
            if (Shell.Current is Shell shell)
            {
                var homeFlyoutItem = shell.Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "HomePage");
                if (homeFlyoutItem != null)
                {
                    if (shell.CurrentItem == homeFlyoutItem)
                        return Task.CompletedTask;
                    shell.CurrentItem = homeFlyoutItem;
                    return Task.CompletedTask;
                }
                throw new System.Exception("Aucune racine HomePage disponible dans le Shell !");
            }
            throw new System.Exception("Shell.Current n'est pas un Shell valide.");
        }

        public Task GoToAdminDashboardPageAsync()
        {
            if (Shell.Current is Shell shell)
            {
                var adminFlyoutItem = shell.Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "AdminDashboardPage");
                if (adminFlyoutItem != null)
                {
                    if (shell.CurrentItem == adminFlyoutItem)
                        return Task.CompletedTask;
                    shell.CurrentItem = adminFlyoutItem;
                    return Task.CompletedTask;
                }
                throw new System.Exception("Aucune racine AdminDashboardPage disponible dans le Shell !");
            }
            throw new System.Exception("Shell.Current n'est pas un Shell valide.");
        }

        public Task GoToStartSessionPageAsync()
            => Shell.Current.GoToAsync("StartSessionPage");

        public Task GoToEndSessionPageAsync()
            => Shell.Current.GoToAsync("EndSessionPage");

        public Task GoToTimeEntriesPageAsync()
            => Shell.Current.GoToAsync("TimeEntriesPage");

        public Task GoBackAsync()
            => Shell.Current.GoToAsync("..");
    }
}