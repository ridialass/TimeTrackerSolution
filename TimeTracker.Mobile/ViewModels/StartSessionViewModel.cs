using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

public partial class StartSessionViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IMobileTimeEntryService _timeEntryService;
    private readonly IGeolocationService _geoService;

    private PausePeriod? currentPause;


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

    public StartSessionViewModel(IAuthService authService, 
        IMobileTimeEntryService timeEntryService, 
        IGeolocationService geoService)
    {
        _authService = authService;
        _timeEntryService = timeEntryService;
        _geoService = geoService;

        StartCommand = new Command(async () => await OnStartSessionAsync());
        PauseCommand = new AsyncRelayCommand(OnPauseAsync);
        ResumeCommand = new AsyncRelayCommand(OnResumeAsync);
    }

    private async Task OnStartSessionAsync()
    {
        var loc = await _geoService.GetCurrentLocationAsync();
        string address = "Localisation indisponible";
        double lat = 0, lon = 0;

        try
        {
            if (loc != null)
            {
                // Récupération de la localisation
                lat = loc.Latitude;
                lon = loc.Longitude;
                address = await _geoService.GetAddressFromCoordinatesAsync(lat, lon);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération de la localisation : {ex.Message}");
        }

        // Vérification de l'utilisateur connecté
        var user = _authService.CurrentUser;
        if (user == null)
        {
            await Shell.Current.DisplayAlert("Erreur", "Aucun utilisateur connecté.", "OK");
            return;
        }

        // Création du DTO
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
            Location = address,
            Pauses = new List<PausePeriod>() // toujours initialisé
        };

        try
        {
            // Appel des services backend
            await _timeEntryService.CreateTimeEntryAsync(dto);
            await _timeEntryService.StartSessionAsync(dto);

            // Navigation vers la page suivante
            await Shell.Current.GoToAsync(nameof(TimeTracker.Mobile.Views.EndSessionPage));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la création de la session : {ex.Message}");
            await Shell.Current.DisplayAlert("Erreur", "Impossible de démarrer la session.", "OK");
            return;
        }
        
    }

    private async Task OnPauseAsync()
    {
        if (IsPaused || currentPause != null) return;

        currentPause = new PausePeriod { Start = DateTime.UtcNow };
        IsPaused = true;

        await Shell.Current.DisplayAlert("Pause", "Pause démarrée", "OK");
    }

    private async Task OnResumeAsync()
    {
        if (!IsPaused || currentPause == null) return;

        currentPause.End = DateTime.UtcNow;

        if (_timeEntryService.InProgressSession?.Pauses == null)
            _timeEntryService.InProgressSession.Pauses = new List<PausePeriod>();

        _timeEntryService.InProgressSession?.Pauses.Add(currentPause);
        currentPause = null;
        IsPaused = false;

        await Shell.Current.DisplayAlert("Reprise", "Pause terminée", "OK");
    }
}
