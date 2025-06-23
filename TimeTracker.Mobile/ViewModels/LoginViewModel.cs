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
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            var result = await _authService.LoginAsync(username, password);
            if (result.IsSuccess)
            {
                var user = _authService.CurrentUser;
                if (user is not null)
                {
                    if (Shell.Current is AppShell shell)
                    {
                        // Ajoute dynamiquement le menu pour le rôle de l'utilisateur
                        shell.ConfigureFlyoutForRole(user.Role);
                        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;

                        // Navigation absolue possible maintenant (car le menu a été ajouté)
                        if (Enum.TryParse<UserRole>(user.Role, out var userRole))
                        {
                            if (userRole == UserRole.Admin)
                                await _navigation.GoToAdminDashboardPageAsync();
                            else
                                await _navigation.GoToHomePageAsync();
                        }
                        else
                        {
                            ErrorMessage = "Rôle utilisateur inconnu après connexion.";
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Impossible de déterminer l'utilisateur après connexion.";
                }
            }
            else
            {
                ErrorMessage = "Échec de la connexion : " + (result.Error ?? "Erreur inconnue");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la connexion : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}