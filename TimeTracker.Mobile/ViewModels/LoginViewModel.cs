// SECURITE :
// Ne jamais logger ni persister le mot de passe utilisateur dans ce service ou ailleurs dans le projet.
// Toujours transmettre les identifiants via HTTPS et uniquement via POST (jamais URL).
// Seul le token JWT peut être stocké localement, pas le mot de passe.

using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Mobile.Resources.Strings; // Ajuste selon ton namespace
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ISessionStateService _sessionService;
    private readonly INavigationService _navigationService;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set
        {
            if (SetProperty(ref username, value))
            {
                OnPropertyChanged(nameof(CanLogin));
                // Masque le message d'erreur si l'utilisateur modifie un champ
                if (!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = string.Empty;
            }
        }
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set
        {
            if (SetProperty(ref password, value))
            {
                OnPropertyChanged(nameof(CanLogin));
                // Masque le message d'erreur si l'utilisateur modifie un champ
                if (!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage = string.Empty;
            }
        }
    }

    public new bool IsBusy
    {
        get => base.IsBusy;
        set
        {
            if (base.IsBusy != value)
            {
                base.IsBusy = value;
                OnPropertyChanged(nameof(CanLogin));
            }
        }
    }

    public bool CanLogin =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && !IsBusy;

    public LoginViewModel(ISessionStateService sessionService, INavigationService navigationService)
    {
        _sessionService = sessionService;
        _navigationService = navigationService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty; // Masquer le message d'erreur pendant la connexion

        try
        {
            var success = await _sessionService.LoginAsync(username, password);
            Password = string.Empty;

            if (!success)
            {
                ErrorMessage = AppResources.Login_Error_Invalid;
                return;
            }

        }
        catch
        {
            ErrorMessage = AppResources.Login_Error_Exception;
            Password = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
}