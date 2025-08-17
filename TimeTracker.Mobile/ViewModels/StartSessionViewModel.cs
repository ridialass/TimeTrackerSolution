using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services.Interfaces;

public partial class StartSessionViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialogs;
    private readonly IApiClientService _apiClient;

    // DTO côté mobile (pas d’entités)
    private PausePeriodDto? currentPause;

    private bool isPaused;
    public bool IsPaused
    {
        get => isPaused;
        set => SetProperty(ref isPaused, value);
    }

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
        IGeolocationService geoService,
        INavigationService navigationService,
        IDialogService dialogService,
        IApiClientService apiClientService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;
        _nav = navigationService;
        _dialogs = dialogService;
        _apiClient = apiClientService;

        // Valeur par défaut raisonnable
        selectedSessionType = WorkSessionType.Regular;

        StartCommand = new AsyncRelayCommand(OnStartSessionAsync);
    }

    private async Task OnStartSessionAsync()
    {
        var inProg = _timeEntryService.InProgressSession;
        if (inProg != null)
        {
            // Harmonisé : Ne jamais envoyer une session incomplète côté API !
            // Si une session existe localement mais n'a pas d'Id serveur, elle est considérée comme "in progress" seulement locale
            await _nav.GoToEndSessionPageAsync();
            return;
        }

        var user = _authService.CurrentUser;
        if (user == null)
        {
            await _dialogs.ShowErrorAsync("Aucun utilisateur connecté.");
            return;
        }

        // Localisation (best effort)
        string address = "Localisation indisponible";
        double lat = 0, lon = 0;
        try
        {
            var loc = await _geoService.GetCurrentLocationAsync();
            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                address = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StartSession] Erreur localisation: {ex.Message}");
        }

        var dto = new TimeEntryDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            SessionType = selectedSessionType,
            StartTime = DateTime.Now,           // on reste cohérent avec l’heure UI
            IncludesTravelTime = includesTravelTime,
            StartLatitude = lat,
            StartLongitude = lon,
            StartAddress = address,
            DinnerPaid = DinnerPaidBy.None,
            Location = address,
            Pauses = new System.Collections.Generic.List<PausePeriodDto>()
        };

        try
        {
            // Harmonisé : Démarrer/mettre en cache la session localement UNIQUEMENT, jamais côté API tant que EndTime n'est pas présent
            await _timeEntryService.StartSessionAsync(dto);

            // Naviguer vers la page de fin
            await _nav.GoToEndSessionPageAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StartSession] Erreur création: {ex.Message}");
            await _dialogs.ShowErrorAsync("Impossible de démarrer la session.");
        }
    }
}