using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class LoginPage : ContentPage
{
        // MAUI expose le provider via App.Current.Handler.MauiContext.Services
    private IMobileAuthService? AuthServiceOrNull =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<IMobileAuthService>();
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    // Nouveau constructeur paramètre-less, utilisé par le XAML et si on fait `new LoginPage()` :
    public LoginPage()
        : this(
            // On récupère le LoginViewModel dans le container MAUI
            Application.Current!
                           .Handler!
                           .MauiContext!
                           .Services
                           .GetRequiredService<LoginViewModel>()
          )
    {
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var auth = AuthServiceOrNull;
        // Si un utilisateur est déjà authentifié (dans AuthService), on le redirige selon son rôle
        if (auth?.CurrentUser is { } user)
        {
            // Si c’est un admin, on va à AdminDashboardPage
            if (auth.CurrentUser.Role == UserRole.Admin)
                Shell.Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
            else
                Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
    }
}