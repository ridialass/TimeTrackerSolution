using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views
{
    public partial class HomePage : ContentPage
    {
        IMobileTimeEntryService _sessionService =>
            Handler?.MauiContext?.Services.GetRequiredService<IMobileTimeEntryService>()
            ?? throw new InvalidOperationException("Le service IMobileTimeEntryService n'est pas disponible.");

        IAuthService _authService =>
            Handler?.MauiContext?.Services.GetRequiredService<IAuthService>()
            ?? throw new InvalidOperationException("Le service IMobileAuthService n'est pas disponible.");


        public HomePage(HomeViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        private async void OnStartSessionClicked(object sender, EventArgs e)
        {
            if (_sessionService.InProgressSession != null)
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
            if (_sessionService.InProgressSession == null)
            {
                await DisplayAlert(
                    "Pas de session en cours",
                    "Aucune session n'est en cours. Veuillez d'abord démarrer une session.",
                    "OK");
                return;
            }

            await Shell.Current.GoToAsync(nameof(EndSessionPage));
        }

        private async void OnHistoryClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(TimeEntriesPage));
        }

        private async void OnAdminDashboardClicked(object sender, EventArgs e)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null
                || !Enum.TryParse<UserRole>(currentUser.Role, out var roleEnum)
                || roleEnum != UserRole.Admin)
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
