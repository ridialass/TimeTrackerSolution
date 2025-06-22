using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class LoginPage : ContentPage
{
        // MAUI expose le provider via App.Current.Handler.MauiContext.Services
    private IAuthService? AuthServiceOrNull =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<IAuthService>();

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();


        var user = AuthServiceOrNull?.CurrentUser;
        if (user is not null)
        {
            // Si c’est un admin, on va à AdminDashboardPage
            if (Enum.TryParse<UserRole>(user.Role, out var role) && role == UserRole.Admin)
                Shell.Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
            else
                Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
    }
}