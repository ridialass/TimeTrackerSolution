using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

public partial class StartSessionViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialogs;

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
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }

    public StartSessionViewModel(
        IAuthService authService,
        IMobileTimeEntryService timeEntryService,
        IGeolocationService geoService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;
        _nav = navigationService;
        _dialogs = dialogService;

        // Valeur par défaut raisonnable
        selectedSessionType = WorkSessionType.Regular;

        StartCommand = new AsyncRelayCommand(OnStartSessionAsync);
        PauseCommand = new AsyncRelayCommand(OnPauseAsync);
        ResumeCommand = new AsyncRelayCommand(OnResumeAsync);
    }

    private async Task OnStartSessionAsync()
    {
        if (_timeEntryService.InProgressSession != null)
        {
            await _dialogs.ShowAlertAsync("Info", "Une session est déjà en cours.", "OK");
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
            // 1) Créer côté API pour obtenir l’Id
            await _timeEntryService.CreateTimeEntryAsync(dto);

            // 2) Démarrer côté client (mémoire + persistance locale)
            await _timeEntryService.StartSessionAsync(dto);

            // 3) Naviguer vers la page de fin
            await _nav.GoToEndSessionPageAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StartSession] Erreur création: {ex.Message}");
            await _dialogs.ShowErrorAsync("Impossible de démarrer la session.");
        }
    }

    private async Task OnPauseAsync()
    {
        if (IsPaused || currentPause != null) return;

        currentPause = new PausePeriodDto { Start = DateTime.Now };
        IsPaused = true;

        await _dialogs.ShowSuccessAsync("Pause démarrée", "Pause");
    }

    private async Task OnResumeAsync()
    {
        if (!IsPaused || currentPause == null) return;

        currentPause.End = DateTime.Now;

        var inProgress = _timeEntryService.InProgressSession;
        if (inProgress == null)
        {
            // Sécurité : pas de session en cours → on reset l’état pause et on informe
            IsPaused = false;
            currentPause = null;
            await _dialogs.ShowAlertAsync("Info", "Aucune session en cours détectée.", "OK");
            return;
        }

        inProgress.Pauses ??= new System.Collections.Generic.List<PausePeriodDto>();
        inProgress.Pauses.Add(currentPause);

        // Re-persister localement la session mise à jour (aucun appel réseau)
        await _timeEntryService.StartSessionAsync(inProgress);

        currentPause = null;
        IsPaused = false;

        await _dialogs.ShowSuccessAsync("Pause terminée", "Reprise");
    }
}
