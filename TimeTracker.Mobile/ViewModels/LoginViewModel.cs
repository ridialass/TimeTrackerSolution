// SECURITE :
// Ne jamais logger ni persister le mot de passe utilisateur dans ce service ou ailleurs dans le projet.
// Toujours transmettre les identifiants via HTTPS et uniquement via POST (jamais URL).
// Seul le token JWT peut être stocké localement, pas le mot de passe.

#nullable enable
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Mobile.Resources.Strings; // Ajuste selon ton namespace
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels
{
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
                    if (!string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = string.Empty;
                    LoginCommand.NotifyCanExecuteChanged();
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
                    if (!string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = string.Empty;
                    LoginCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool CanLogin =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !IsBusy;

        public LoginViewModel(ISessionStateService sessionService, INavigationService navigationService)
        {
            _sessionService = sessionService;
            _navigationService = navigationService;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        public async Task LoginAsync()
        {
            // Prépare l’UI
            ErrorMessage = string.Empty;
            IsBusy = true;
            LoginCommand.NotifyCanExecuteChanged();

            try
            {
                var user = (Username ?? string.Empty).Trim();
                var pass = Password ?? string.Empty;

                var success = await _sessionService.LoginAsync(user, pass);

                // Toujours nettoyer le champ mot de passe après tentative
                Password = string.Empty;

                if (!success)
                {
                    ErrorMessage = AppResources.Login_Error_Invalid;
                    return;
                }

                // Navigation selon le rôle (Admin -> Dashboard, sinon Home)
                var role = _sessionService.CurrentUserRole;
                if (!string.IsNullOrWhiteSpace(role) &&
                    role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    await _navigationService.GoToAdminDashboardPageAsync();
                }
                else
                {
                    await _navigationService.GoToHomePageAsync();
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
                LoginCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
