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
                var flyoutItem = shell.CurrentItem as FlyoutItem;
                if (flyoutItem?.Route == "LoginPage")
                    return Task.CompletedTask; // Déjà sur LoginPage

                // Sélectionne LoginPage comme item actif si présent
                var loginFlyoutItem = shell.Items.OfType<FlyoutItem>().FirstOrDefault(i => i.Route == "LoginPage");
                if (loginFlyoutItem != null)
                    shell.CurrentItem = loginFlyoutItem;
            }
            return Shell.Current.GoToAsync("//LoginPage");
        }

        public Task GoToHomePageAsync()
        {
            if (Shell.Current is Shell shell)
            {
                var flyoutItem = shell.CurrentItem as FlyoutItem;
                if (flyoutItem?.Route == "HomePage")
                    return Task.CompletedTask;
            }
            return Shell.Current.GoToAsync("//HomePage");
        }

        public Task GoToAdminDashboardPageAsync()
        {
            if (Shell.Current is Shell shell)
            {
                var flyoutItem = shell.CurrentItem as FlyoutItem;
                if (flyoutItem?.Route == "AdminDashboardPage")
                    return Task.CompletedTask;
            }
            return Shell.Current.GoToAsync("//AdminDashboardPage");
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