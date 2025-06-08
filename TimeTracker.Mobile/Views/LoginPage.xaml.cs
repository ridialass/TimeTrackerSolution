using TimeTracker.Core.Enums;
using static TimeTracker.Models.Enum;

namespace TimeTracker.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Si un utilisateur est d�j� authentifi� (dans AuthService), on le redirige selon son r�le
        if (App.AuthService.CurrentUser != null)
        {
            // Si c�est un admin, on va � AdminDashboardPage
            if (App.AuthService.CurrentUser.Role == UserRole.Admin)
                Shell.Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
            else
                Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
    }
}