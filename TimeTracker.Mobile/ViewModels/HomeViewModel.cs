using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Resources.Strings;

namespace TimeTracker.Mobile.ViewModels;

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
    }

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
    private async Task GoToHistoryAsync()
    {
        await _navigationService.GoToTimeEntriesPageAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (Application.Current is App app)
            await app.LogoutAsync();
    }
}