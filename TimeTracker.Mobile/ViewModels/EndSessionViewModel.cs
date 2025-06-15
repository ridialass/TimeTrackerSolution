using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Helpers;
using TimeTracker.Mobile.Services;
using TimeTracker.Mobile.Views;

namespace TimeTracker.Mobile.ViewModels
{
    public class EndSessionViewModel : INotifyPropertyChanged
    {
        // ── SERVICES INJECTÉS ────────────────────────────────────────
        private readonly IMobileTimeEntryService _timeEntryService;
        private readonly LocationService _locationService;

        // ── COMMANDE PUBLIQUE ───────────────────────────────────────
        public ICommand EndCommand { get; }

        // ════ CONSTRUCTEUR ════
        public EndSessionViewModel(
            IMobileTimeEntryService timeEntryService,
            LocationService locationService)
        {
            _timeEntryService = timeEntryService;
            _locationService = locationService;

            // on bind l’action async au bouton
            EndCommand = new Command(async () => await OnEndSessionAsync());
        }

        // ════ PROPRIÉTÉS PUBLIQUES POUR LE XAML ════

        /// <summary>
        /// La session en cours (nulle si pas de session démarrée).
        /// </summary>
        public TimeEntryDto? InProgressSession => _timeEntryService.InProgressSession;

        /// <summary>
        /// Affiche le résumé type + heure de début.
        /// </summary>
        public string InProgressSessionInfo =>
            InProgressSession != null
                ? $"{InProgressSession.SessionType} – démarrée à {InProgressSession.StartTime:g}"
                : "Aucune session en cours";

        /// <summary>
        /// Si la session en cours inclut un travel, on affiche les champs heures/minutes.
        /// </summary>
        public bool InProgressSessionIncludesTravel =>
            InProgressSession?.IncludesTravelTime == true;

        // Champs pour la saisie du travel
        private string _travelHours = "";
        public string TravelHours
        {
            get => _travelHours;
            set => SetProperty(ref _travelHours, value);
        }

        private string _travelMinutes = "";
        public string TravelMinutes
        {
            get => _travelMinutes;
            set => SetProperty(ref _travelMinutes, value);
        }

        // Choix de qui paie le dîner
        public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
            = new ObservableCollection<DinnerPaidBy>(
                Enum.GetValues<DinnerPaidBy>());

        private DinnerPaidBy _selectedDinnerPaidBy = DinnerPaidBy.None;
        public DinnerPaidBy SelectedDinnerPaidBy
        {
            get => _selectedDinnerPaidBy;
            set => SetProperty(ref _selectedDinnerPaidBy, value);
        }

        // ════ MÉTHODE D’ENDSESSION ════
        private async Task OnEndSessionAsync()
        {
            var session = InProgressSession;
            if (session == null)
                return; // rien à faire

            // 1) Position GPS de fin
            var loc = await _locationService.GetCurrentLocationAsync();
            string endAddress = "Localisation indisponible";
            double lat = 0, lon = 0;
            if (loc != null)
            {
                lat = loc.Latitude;
                lon = loc.Longitude;
                endAddress = await LocationHelper
                    .GetAddressFromCoordinatesAsync(lat, lon);
            }

            // 2) Calcul du travel time si demandé
            if (session.IncludesTravelTime
                && int.TryParse(TravelHours, out var h)
                && int.TryParse(TravelMinutes, out var m))
            {
                session.TravelDurationHours = h + (m / 60.0);
            }
            else
            {
                session.TravelDurationHours = null;
            }

            // 3) Compléter la session
            session.EndTime = DateTime.Now;
            session.EndLatitude = lat;
            session.EndLongitude = lon;
            session.EndAddress = endAddress;
            session.DinnerPaid = SelectedDinnerPaidBy;

            // 4) Persister et libérer la session en cours
            await _timeEntryService.EndAndSaveCurrentSessionAsync();

            // 5) Retour à la page d’accueil
            await Shell.Current.GoToAsync(nameof(HomePage));
        }

        // ════ INotifyPropertyChanged ════
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
    }
}
