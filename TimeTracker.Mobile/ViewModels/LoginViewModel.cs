using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Views;
using static TimeTracker.Models.Enum;

namespace TimeTracker.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = App.AuthService;

        // Champs liés au XAML
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasErrorMessage)); }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        // Commande Login
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLogin);
        }

        private async void OnLogin()
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

            // Authentification réussie, on navigue vers AppShell racine
            // Si c’est un admin, on peut afficher l’Admin Dashboard, sinon la Home normale
            if (_authService.CurrentUser.Role == UserRole.Admin)
            {
                // Navigation relative, sans "//"
                await Shell.Current.GoToAsync(nameof(AdminDashboardPage));
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(HomePage));
            }
        }

        // INotifyPropertyChanged standard
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
