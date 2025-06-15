using TimeTracker.Core.Enums;
using TimeTracker.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.Views;

public partial class AdminDashboardPage : ContentPage
{
    // Stockez la VM injectée
    private readonly AdminDashboardViewModel _vm;

    public AdminDashboardPage(AdminDashboardViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1) Vérifier qu'on a bien un utilisateur dans la VM
        var currentUser = _vm.CurrentUser;
        if (currentUser == null)
        {
            await DisplayAlert(
                "Accès refusé",
                "Vous devez vous connecter.",
                "OK");
            await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        // 2) Vérifier que c'est un Admin
        if (currentUser.Role != UserRole.Admin)
        {
            await DisplayAlert(
                "Accès réservé aux administrateurs",
                "Vous n'avez pas les droits pour accéder à cette page.",
                "OK");
            await Shell.Current.GoToAsync(nameof(HomePage));
            return;
        }

        // Tout est bon, la page s'affiche
    }
}
