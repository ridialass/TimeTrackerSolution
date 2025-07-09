using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Resources.Strings; // Ajout pour i18n

namespace TimeTracker.Mobile.ViewModels;

public partial class StartSessionViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;

    public ObservableCollection<WorkSessionType> SessionTypes { get; }
        = new(Enum.GetValues<WorkSessionType>());

    private WorkSessionType selectedSessionType;
    public WorkSessionType SelectedSessionType
    {
        get => selectedSessionType;
        set => SetProperty(ref selectedSessionType, value);
    }

    private bool includesTravelTime;
    public bool IncludesTravelTime
    {
        get => includesTravelTime;
        set => SetProperty(ref includesTravelTime, value);
    }

    public ICommand StartCommand { get; }

    public StartSessionViewModel(
        IAuthService authService,
        IMobileTimeEntryService timeEntryService,
        IGeolocationService geoService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;

        StartCommand = new Command(async () => await OnStartSessionAsync());
    }

    private async Task OnStartSessionAsync()
    {
        var loc = await _geoService.GetCurrentLocationAsync();
        string address = AppResources.StartSession_LocationUnavailable;
        double lat = 0, lon = 0;

        if (loc != null)
        {
            lat = loc.Latitude;
            lon = loc.Longitude;
            address = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
        }

        var user = _authService.CurrentUser;
        if (user == null)
        {
            await Shell.Current.DisplayAlert(
                AppResources.StartSession_Error_Title,
                AppResources.StartSession_Error_NoUser,
                AppResources.StartSession_Error_OK);
            return;
        }

        var dto = new TimeEntryDto
        {
            UserId = user.Id,
            Username = user.UserName!,
            SessionType = selectedSessionType,
            StartTime = DateTime.Now,
            IncludesTravelTime = includesTravelTime,
            StartLatitude = lat,
            StartLongitude = lon,
            StartAddress = address,
            DinnerPaid = DinnerPaidBy.None,
            Location = address
        };

        try
        {
            // Appel API pour sauvegarder en base
            await _timeEntryService.CreateTimeEntryAsync(dto);

            // ✅ IMPORTANT : enregistrer la session en mémoire (sinon EndSessionPage ne la voit pas)
            await _timeEntryService.StartSessionAsync(dto);
        }
        catch
        {
            await Shell.Current.DisplayAlert(
                AppResources.StartSession_Error_Title,
                AppResources.StartSession_Error_CannotStart,
                AppResources.StartSession_Error_OK);
            return;
        }

        // Navigation
        await Shell.Current.GoToAsync(nameof(TimeTracker.Mobile.Views.EndSessionPage));
    }
}