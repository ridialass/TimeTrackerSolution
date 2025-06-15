using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Helpers;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WorkSessionType> SessionTypes { get; }
            = new ObservableCollection<WorkSessionType>(
                (WorkSessionType[])Enum.GetValues(typeof(WorkSessionType)));

        public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
            = new ObservableCollection<DinnerPaidBy>(
                (DinnerPaidBy[])Enum.GetValues(typeof(DinnerPaidBy)));

        WorkSessionType _selectedSessionType;
        public WorkSessionType SelectedSessionType
        {
            get => _selectedSessionType;
            set => SetProperty(ref _selectedSessionType, value);
        }

        bool _includesTravelTime;
        public bool IncludesTravelTime
        {
            get => _includesTravelTime;
            set => SetProperty(ref _includesTravelTime, value);
        }

        string _travelHours = "";
        public string TravelHours
        {
            get => _travelHours;
            set => SetProperty(ref _travelHours, value);
        }

        string _travelMinutes = "";
        public string TravelMinutes
        {
            get => _travelMinutes;
            set => SetProperty(ref _travelMinutes, value);
        }

        DinnerPaidBy _selectedDinnerPaidBy = DinnerPaidBy.None;
        public DinnerPaidBy SelectedDinnerPaidBy
        {
            get => _selectedDinnerPaidBy;
            set => SetProperty(ref _selectedDinnerPaidBy, value);
        }

        public ICommand ClockInCommand { get; }
        public ICommand ClockOutCommand { get; }
        public ICommand GoToHistoryCommand { get; }

        TimeEntryDto? _currentEntry;
        readonly LocationService _locationService;
        readonly IMobileAuthService _authService;
        readonly IMobileTimeEntryService _timeEntryService;

        public HomeViewModel(
            IMobileAuthService authService,
            IMobileTimeEntryService timeEntryService)
        {
            _authService = authService;
            _timeEntryService = timeEntryService;
            _locationService = new LocationService();

            ClockInCommand = new Command(async () => await OnClockInAsync());
            ClockOutCommand = new Command(async () => await OnClockOutAsync());
            GoToHistoryCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(TimeEntryPage)));
        }

        public bool IsCurrentUserAdmin =>
            _authService.CurrentUser?.Role == UserRole.Admin;

        async Task OnClockInAsync()
        {
            var loc = await _locationService.GetCurrentLocationAsync();
            var addr = "Location unavailable";
            double lat = 0, lon = 0;

            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                addr = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // Crée un DTO immuable
            _currentEntry = new TimeEntryDto
            {
                UserId = _authService.CurrentUser!.Id,
                Username = _authService.CurrentUser.UserName!,
                SessionType = SelectedSessionType,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                StartLatitude = lat,
                StartLongitude = lon,
                StartAddress = addr,
                EndLatitude = null,
                EndLongitude = null,
                EndAddress = null,
                IncludesTravelTime = IncludesTravelTime,
                // ce champ n’existe plus, on ne l’utilise pas
                // TravelDurationHours = null,
                DinnerPaid = DinnerPaidBy.None,
                Location = addr
            };

            TravelHours = "";
            TravelMinutes = "";
        }

        async Task OnClockOutAsync()
        {
            if (_currentEntry is null) return;

            var loc = await _locationService.GetCurrentLocationAsync();
            var endAddr = "Location unavailable";
            double lat = 0, lon = 0;

            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                endAddr = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // Calcule le TimeSpan de trajet
            TimeSpan? travelSpan = null;
            if (IncludesTravelTime
                && int.TryParse(TravelHours, out var h)
                && int.TryParse(TravelMinutes, out var m))
            {
                travelSpan = TimeSpan.FromHours(h + m / 60.0);
            }

            // Crée un nouvel objet TimeEntryDto sans essayer de modifier une propriété en lecture seule
            var completed = new TimeEntryDto
            {
                Id = _currentEntry.Id,
                StartTime = _currentEntry.StartTime,
                EndTime = DateTime.UtcNow,
                StartLatitude = _currentEntry.StartLatitude,
                StartLongitude = _currentEntry.StartLongitude,
                StartAddress = _currentEntry.StartAddress,
                EndLatitude = lat,
                EndLongitude = lon,
                EndAddress = endAddr,
                SessionType = _currentEntry.SessionType,
                IncludesTravelTime = _currentEntry.IncludesTravelTime,
                TravelDurationHours = _currentEntry.TravelDurationHours,
                DinnerPaid = SelectedDinnerPaidBy,
                Location = _currentEntry.Location,
                UserId = _currentEntry.UserId,
                Username = _currentEntry.Username
            };

            // Utilise travelSpan pour des calculs ou des journaux si nécessaire, mais ne l'assigne pas directement
            if (travelSpan.HasValue)
            {
                // Exemple : journalisation ou traitement supplémentaire
                Console.WriteLine($"Durée estimée du trajet : {travelSpan.Value}");
            }

            try
            {
                await _timeEntryService.CreateTimeEntryAsync(completed);
            }
            catch
            {
                // TODO : afficher un message
            }
            finally
            {
                _currentEntry = null;
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
        #endregion
    }
}
