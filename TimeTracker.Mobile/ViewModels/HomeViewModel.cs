using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    // ... Existing observable properties ...

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
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (_timeEntryService.InProgressSession != null)
        {
            await _dialogService.ShowAlertAsync(
                "Session en cours",
                "Vous avez déjà une session non terminée. Veuillez d'abord terminer cette session.",
                "OK");
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
                "Pas de session en cours",
                "Aucune session n'est en cours. Veuillez d'abord démarrer une session.",
                "OK");
            return;
        }
        await _navigationService.GoToEndSessionPageAsync();
    }

    [RelayCommand]
    private async Task GoToHistoryAsync()
    {
        await _navigationService.GoToTimeEntriesPageAsync();
    }

    [RelayCommand]
    private async Task GoToAdminDashboardAsync()
    {
        var currentUser = _authService.CurrentUser;
        // Correction: Safely parse string role to enum for comparison
        if (currentUser == null ||
            !System.Enum.TryParse<UserRole>(currentUser.Role, out var roleEnum) ||
            roleEnum != UserRole.Admin)
        {
            await _dialogService.ShowAlertAsync(
                "Accès refusé",
                "Vous n'avez pas les droits pour accéder à l'Admin Dashboard.",
                "OK");
            return;
        }
        await _navigationService.GoToAdminDashboardPageAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (Application.Current is App app)
            await app.LogoutAsync();
    }

    // Correction: Safely compare to string "Admin" or use enum if you want
    public bool IsCurrentUserAdmin
    {
        get
        {
            var role = _authService.CurrentUser?.Role;
            return System.Enum.TryParse<UserRole>(role, out var roleEnum) && roleEnum == UserRole.Admin;
        }
    }

    // ... Any additional properties/methods ...
}