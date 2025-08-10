// TimeTracker.Mobile/ViewModels/HomeViewModel.cs
#nullable enable
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Resources.Strings;
using TimeTracker.Mobile.Services;
using Microsoft.Maui.Controls;

namespace TimeTracker.Mobile.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IMobileTimeEntryService _timeEntryService;
        private readonly IGeolocationService _geoService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public HomeViewModel(
            IAuthService authService,
            IMobileTimeEntryService timeEntryService,
            IGeolocationService geoService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _timeEntryService = timeEntryService;
            _geoService = geoService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            // Init l'état “admin” au démarrage
            UpdateIsCurrentUserAdmin();
        }

        // --- Etat dérivé : admin ou pas (avec notifications) ---
        private bool isCurrentUserAdmin;
        public bool IsCurrentUserAdmin
        {
            get => isCurrentUserAdmin;
            private set => SetProperty(ref isCurrentUserAdmin, value);
        }

        private void UpdateIsCurrentUserAdmin()
        {
            var roleStr = _authService.CurrentUser?.Role;
            IsCurrentUserAdmin = Enum.TryParse<UserRole>(roleStr, out var role) && role == UserRole.Admin;
        }

        /// <summary>
        /// A appeler depuis OnAppearing/OnNavigatedTo de la page.
        /// Met à jour l’état (ex. si l’auth a changé en arrière-plan).
        /// </summary>
        [RelayCommand]
        public Task RefreshAsync()
        {
            UpdateIsCurrentUserAdmin();
            return Task.CompletedTask;
        }

        // --- Commandes de navigation/actions ---

        [RelayCommand]
        private async Task StartSessionAsync()
        {
            if (_timeEntryService.InProgressSession != null)
            {
                await _dialogService.ShowAlertAsync(
                    AppResources.Home_StartSession_Alert_Title,
                    AppResources.Home_StartSession_Alert_AlreadyStarted,
                    AppResources.Home_OK);
                return;
            }
            await _navigationService.GoToStartSessionPageAsync();
        }

        [RelayCommand]
        private async Task EndSessionAsync()
        {
            if (_timeEntryService.InProgressSession == null)
            {
                await _dialogService.ShowAlertAsync(
                    AppResources.Home_EndSession_Alert_Title,
                    AppResources.Home_EndSession_Alert_NoSession,
                    AppResources.Home_OK);
                return;
            }
            await _navigationService.GoToEndSessionPageAsync();
        }

        [RelayCommand]
        private Task GoToHistoryAsync() => _navigationService.GoToTimeEntriesPageAsync();

        [RelayCommand]
        private async Task GoToAdminDashboardAsync()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null ||
                !Enum.TryParse<UserRole>(currentUser.Role, out var roleEnum) ||
                roleEnum != UserRole.Admin)
            {
                await _dialogService.ShowAlertAsync(
                    AppResources.Home_AdminDashboard_Alert_Title,
                    AppResources.Home_AdminDashboard_Alert_AccessDenied,
                    AppResources.Home_OK);
                return;
            }
            await _navigationService.GoToAdminDashboardPageAsync();
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Si ton App expose déjà un Logout orchestré, on le garde
            if (Application.Current is App app)
            {
                await app.LogoutAsync();
                return;
            }

            // Fallback sans App.LogoutAsync :
            await _authService.LogoutAsync();
            await _navigationService.GoToLoginPageAsync();
        }
    }
}
