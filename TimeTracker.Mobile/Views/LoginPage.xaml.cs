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
        //Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    // ✅ Clear any previous error messages when the page appears
    //    if (BindingContext is LoginViewModel vm)
    //        vm.ErrorMessage = string.Empty;
    //    // ✅ Re-enable the Flyout when navigating back to LoginPage
    //    Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    //}
}