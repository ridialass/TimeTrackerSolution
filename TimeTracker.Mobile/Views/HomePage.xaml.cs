using Microsoft.Maui.Controls;
using TimeTracker.Core.Enums;
using static TimeTracker.Models.Enum;

namespace TimeTracker.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnStartSessionClicked(object sender, EventArgs e)
        {
            // Si on a déjà une session en cours, on force l'utilisateur à terminer
            if (App.SessionService.InProgressSession != null)
            {
                await DisplayAlert(
                    "Session en cours",
                    "Vous avez déjà une session non terminée. Veuillez d'abord terminer cette session.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(StartSessionPage));
        }

        private async void OnEndSessionClicked(object sender, EventArgs e)
        {
            if (App.SessionService.InProgressSession == null)
            {
                await DisplayAlert(
                    "Pas de session en cours",
                    "Aucune session n'est en cours. Veuillez d'abord démarrer une session.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(EndSessionPage));
        }


        private async void OnHistoryClicked(object sender, System.EventArgs e)
        {
            // Navigue vers la page SessionHistoryPage
            await Shell.Current.GoToAsync(nameof(SessionHistoryPage));
        }

        private async void OnAdminDashboardClicked(object sender, EventArgs e)
        {
            var currentUser = App.AuthService.CurrentUser;
            if (currentUser == null || currentUser.Role != UserRole.Admin)
            {
                await DisplayAlert(
                    "Accès refusé",
                    "Vous n'avez pas les droits pour accéder à l'Admin Dashboard.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
        }

    }
}
