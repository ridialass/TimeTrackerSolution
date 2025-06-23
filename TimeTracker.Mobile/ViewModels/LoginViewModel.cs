using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigation;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public LoginViewModel(IAuthService authService, INavigationService navigation)
    {
        _authService = authService;
        _navigation = navigation;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Forcer désactivation du menu au début
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            var result = await _authService.LoginAsync(username, password);
            if (result.IsSuccess)
            {
                var user = _authService.CurrentUser;
                if (user is not null && Enum.TryParse<UserRole>(user.Role, out var role))
                {
                    // Réactiver le Flyout uniquement après succès
                    Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

                    if (Shell.Current is AppShell shell)
                        shell.ConfigureFlyoutForRole(user.Role);

                    if (role == UserRole.Admin)
                        await _navigation.GoToAdminDashboardPageAsync();
                    else
                        await _navigation.GoToHomePageAsync();
                }
                else
                {
                    ErrorMessage = "Could not determine user role after login.";
                }
            }
            else
            {
                ErrorMessage = "Login failed: " + (result.Error ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
