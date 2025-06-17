using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IMobileAuthService _authService;

        public LoginViewModel(IMobileAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await OnLoginAsync());
        }

        // —————— Champs liés au XAML ——————
        private string _username = "";
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // **NOUVEAU** :
        public ApplicationUser? CurrentUser => _authService.CurrentUser;

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        // —————— Commande Login ——————
        public ICommand LoginCommand { get; }

        private async Task OnLoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez entrer un nom d’utilisateur et un mot de passe.";
                return;
            }

            var success = await _authService.LoginAsync(Username.Trim(), Password);
            if (!success)
            {
                ErrorMessage = "Identifiants invalides.";
                return;
            }

            // Selon le rôle, navigue vers la page adéquate
            if (_authService.CurrentUser?.Role == UserRole.Admin)
                await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
            else
                await Shell.Current.GoToAsync(nameof(HomePage));
        }

        // —————— INotifyPropertyChanged ——————
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                OnPropertyChanged(name);
            }
        }
    }
}
