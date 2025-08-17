// SECURITE :
// - Ne jamais logger ni persister le mot de passe utilisateur.
// - Identifiants via HTTPS + POST uniquement.
// - Seul le token JWT peut être stocké localement (jamais le mot de passe).

#nullable enable
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class RegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        // Regex simple et robuste (pas exhaustive RFC, mais pratique en UI)
        // - Autorise lettres/chiffres + . _ % + -
        // - Domaine avec points
        // - TLD 2 à 63 chars
        private static readonly Regex EmailRegex = new(
            @"^[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,63}$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public RegistrationViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        // ------- Champs saisis -------
        private string username = string.Empty;
        public string Username
        {
            get => username;
            set
            {
                if (SetProperty(ref username, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
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
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string email = string.Empty;
        public string Email
        {
            get => email;
            set
            {
                if (SetProperty(ref email, value))
                {
                    // Validation à la volée
                    IsEmailValid = EmailRegex.IsMatch((value ?? string.Empty).Trim());
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string firstName = string.Empty;
        public string FirstName
        {
            get => firstName;
            set
            {
                if (SetProperty(ref firstName, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string lastName = string.Empty;
        public string LastName
        {
            get => lastName;
            set
            {
                if (SetProperty(ref lastName, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string town = string.Empty;
        public string Town
        {
            get => town;
            set
            {
                if (SetProperty(ref town, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string country = string.Empty;
        public string Country
        {
            get => country;
            set
            {
                if (SetProperty(ref country, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string role = string.Empty;
        public string Role
        {
            get => role;
            set
            {
                if (SetProperty(ref role, value))
                {
                    ClearMessages();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        // ------- Etat validation -------
        private bool isEmailValid;
        public bool IsEmailValid
        {
            get => isEmailValid;
            private set => SetProperty(ref isEmailValid, value);
        }

        // ------- Messages UI -------
        private string registrationSuccess = string.Empty;
        public string RegistrationSuccess
        {
            get => registrationSuccess;
            set => SetProperty(ref registrationSuccess, value);
        }

        // ------- CanExecute -------
        public bool CanRegister =>
            !IsBusy &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(Email) &&
            IsEmailValid;

        // ------- Command -------
        [RelayCommand(CanExecute = nameof(CanRegister))]
        public async Task RegisterAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            RegistrationSuccess = string.Empty;
            RegisterCommand.NotifyCanExecuteChanged();

            try
            {
                var roleToUse = UserRole.Employee;
                if (!string.IsNullOrWhiteSpace(Role) &&
                    Enum.TryParse<UserRole>(Role, true, out var parsed))
                {
                    roleToUse = parsed;
                }

                var dto = new RegisterRequestDto
                {
                    Username = (Username ?? string.Empty).Trim(),
                    Password = Password ?? string.Empty, // ⚠️ sera vidé après tentative
                    Email = (Email ?? string.Empty).Trim(),
                    FirstName = (FirstName ?? string.Empty).Trim(),
                    LastName = (LastName ?? string.Empty).Trim(),
                    Town = (Town ?? string.Empty).Trim(),
                    Country = (Country ?? string.Empty).Trim(),
                    Role = roleToUse
                };

                var result = await _authService.RegisterAsync(dto);

                // Toujours nettoyer le mot de passe après tentative
                Password = string.Empty;

                if (result.IsSuccess)
                {
                    RegistrationSuccess = AppResources.Registration_Success;
                }
                else
                {
                    ErrorMessage = result.Error ?? AppResources.Registration_Failed;
                }
            }
            catch
            {
                ErrorMessage = AppResources.Registration_Failed;
                Password = string.Empty;
            }
            finally
            {
                IsBusy = false;
                RegisterCommand.NotifyCanExecuteChanged();
            }
        }

        private void ClearMessages()
        {
            if (!string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = string.Empty;
            if (!string.IsNullOrEmpty(RegistrationSuccess)) RegistrationSuccess = string.Empty;
        }
    }
}
