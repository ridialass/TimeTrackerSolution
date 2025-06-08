using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Helpers;
using Microsoft.Maui.Devices.Sensors;
using static TimeTracker.Models.Enum;
using TimeTracker.Views;

namespace TimeTracker.ViewModels
{
    public class EndSessionViewModel : INotifyPropertyChanged
    {
        // ════ Données de session en cours ════
        private readonly SessionService _sessionService = App.SessionService;

        // Expose l’instance en cours
        public WorkSession InProgress => _sessionService.InProgressSession;

        // ════ PROPRIÉTÉS BINDÉES ════

        // Pour afficher un petit résumé (session type + heure de début)
        public string InProgressSessionInfo =>
            InProgress != null
                ? $"{InProgress.SessionType} - Started at {InProgress.StartTime:G}"
                : "No session in progress";

        // Si la session en cours inclut le travel, on rend visible les deux champs
        public bool InProgressSessionIncludesTravel =>
            InProgress?.IncludesTravelTime == true;

        // Champs Heures / Minutes de trajet
        private string _travelHours;
        public string TravelHours
        {
            get => _travelHours;
            set => SetProperty(ref _travelHours, value);
        }

        private string _travelMinutes;
        public string TravelMinutes
        {
            get => _travelMinutes;
            set => SetProperty(ref _travelMinutes, value);
        }

        // Choix de qui paye le dîner
        public ObservableCollection<DinnerPaidBy> DinnerPaidByOptions { get; }
            = new ObservableCollection<DinnerPaidBy>(
                (DinnerPaidBy[])System.Enum.GetValues(typeof(DinnerPaidBy)));

        private DinnerPaidBy _selectedDinnerPaidBy = DinnerPaidBy.None;
        public DinnerPaidBy SelectedDinnerPaidBy
        {
            get => _selectedDinnerPaidBy;
            set => SetProperty(ref _selectedDinnerPaidBy, value);
        }

        // ════ COMMANDE ════
        public ICommand EndCommand { get; }

        // ════ SERVICE/LIBRAIRIES ════
        private readonly LocationService _locationService = new LocationService();

        public EndSessionViewModel()
        {
            EndCommand = new Command(OnEndSession);
        }

        private async void OnEndSession()
        {
            if (InProgress == null)
                return; // pas de session en cours

            // 1) Récupérer la position GPS de fin
            var location = await _locationService.GetCurrentLocationAsync();

            string endAddress = "Location unavailable";
            double lat = 0, lon = 0;

            if (location != null)
            {
                lat = location.Latitude;
                lon = location.Longitude;
                endAddress = await LocationHelper.GetAddressFromCoordinatesAsync(lat, lon);
            }

            // 2) Calculer TravelDurationHours si nécessaire
            if (InProgress.IncludesTravelTime
                && int.TryParse(TravelHours, out var h)
                && int.TryParse(TravelMinutes, out var m))
            {
                InProgress.TravelDurationHours = h + (m / 60.0);
            }
            else
            {
                InProgress.TravelDurationHours = null;
            }

            // 3) Finaliser la session
            InProgress.EndTime = DateTime.Now;
            InProgress.EndLatitude = lat;
            InProgress.EndLongitude = lon;
            InProgress.EndAddress = endAddress;
            InProgress.DinnerPaid = SelectedDinnerPaidBy;

            // 4) Sauvegarder en DB et supprimer la session en cours des Preferences
            await _sessionService.EndAndSaveCurrentSessionAsync();

            // 5) Revenir à la racine HomePage
            await Shell.Current.GoToAsync(nameof(HomePage));
        }

        // ════ INotifyPropertyChanged ════
        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
