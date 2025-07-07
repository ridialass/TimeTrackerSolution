using TimeTracker.Mobile.ViewModels;

namespace TimeTracker.Mobile.Views;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    private async void OnBackToHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        if (Application.Current is App app)
            await app.LogoutAsync();
    }
}