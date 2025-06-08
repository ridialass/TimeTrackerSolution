using TimeTracker.Core.Enums;
using static TimeTracker.Models.Enum;

namespace TimeTracker.Views;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1) Vérifier s’il y a vraiment un utilisateur connecté
        var currentUser = App.AuthService.CurrentUser;
        if (currentUser == null)
        {
            // Si personne n’est connecté, rediriger vers Login
            await Shell.Current.DisplayAlert(
                "Accès refusé",
                "Vous devez vous connecter.",
                "OK");
            await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        // 2) Vérifier que le rôle est bien Admin
        if (currentUser.Role != UserRole.Admin)
        {
            // Si ce n’est pas un Admin, on affiche un message puis on redirige
            await Shell.Current.DisplayAlert(
                "Accès réservé aux administrateurs",
                "Vous n'avez pas les droits pour accéder à cette page.",
                "OK");
            // Rediriger vers HomePage
            await Shell.Current.GoToAsync(nameof(HomePage));
            return;
        }

        // Si on est ici, c’est que l’utilisateur est bien Admin → on laisse la page s’afficher
    }
}