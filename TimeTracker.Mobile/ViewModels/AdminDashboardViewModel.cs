using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Mobile.Services;

namespace TimeTracker.Mobile.ViewModels
{
    public class AdminDashboardViewModel : INotifyPropertyChanged
    {
        // ─── Services injectés ───────────────────────────────────────
        private readonly IApiClientService _apiClient;
        private readonly IMobileAuthService _authService;
        private readonly IMobileTimeEntryService _sessionService;

        // ─── Collections exposées à la vue ───────────────────────────
        public ObservableCollection<UserRole> Roles { get; }
            = new ObservableCollection<UserRole>(Enum.GetValues<UserRole>());

        public ObservableCollection<EmployeeDto> AllUsers { get; }
            = new ObservableCollection<EmployeeDto>();

        public ObservableCollection<TimeEntryDto> FilteredEntries { get; }
            = new ObservableCollection<TimeEntryDto>();

        // ─── Propriétés pour la création d’utilisateur ───────────────
        private string _newUsername = "";
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private UserRole _newRole = UserRole.Technician;
        public UserRole NewRole
        {
            get => _newRole;
            set => SetProperty(ref _newRole, value);
        }

        private string _createError = "";
        public string CreateError
        {
            get => _createError;
            set { SetProperty(ref _createError, value); OnPropertyChanged(nameof(HasCreateError)); }
        }
        public bool HasCreateError => !string.IsNullOrEmpty(CreateError);

        private string _createSuccess = "";
        public string CreateSuccess
        {
            get => _createSuccess;
            set { SetProperty(ref _createSuccess, value); OnPropertyChanged(nameof(HasCreateSuccess)); }
        }
        public bool HasCreateSuccess => !string.IsNullOrEmpty(CreateSuccess);

        // ─── Filtre des sessions ─────────────────────────────────────
        private EmployeeDto? _selectedUser;
        public EmployeeDto? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private string _exportStatus = "";
        public string ExportStatus
        {
            get => _exportStatus;
            set { SetProperty(ref _exportStatus, value); OnPropertyChanged(nameof(HasExportStatus)); }
        }
        public bool HasExportStatus => !string.IsNullOrEmpty(ExportStatus);

        // ─── Commandes liées aux boutons ─────────────────────────────
        public ICommand CreateUserCommand { get; }
        public ICommand LoadSessionsByUserCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        /// <summary>
        /// Note : on peut tester directement CurrentUser == null côté View si besoin,
        ///     mais on expose ici le user restauré pour la vue.
        /// </summary>
        public ApplicationUser? CurrentUser => _authService.CurrentUser;

        public AdminDashboardViewModel(
            IApiClientService apiClient,
            IMobileAuthService authService,
            IMobileTimeEntryService sessionService)
        {
            _apiClient = apiClient;
            _authService = authService;
            _sessionService = sessionService;

            CreateUserCommand = new Command(async () => await OnCreateUser());
            LoadSessionsByUserCommand = new Command(async () => await OnLoadSessionsByUser());
            ExportToCsvCommand = new Command(async () => await OnExportToCsv());

            // Chargement initial de tous les utilisateurs
            _ = LoadAllUsersAsync();
        }

        // ──────────────────────────────────────────────────────────────
        // 1) Charger tous les utilisateurs
        // ──────────────────────────────────────────────────────────────
        private async Task LoadAllUsersAsync()
        {
            try
            {
                AllUsers.Clear();
                var users = await _apiClient.GetEmployeesAsync();
                foreach (var u in users.OrderBy(u => u.Username))
                    AllUsers.Add(u);
            }
            catch (Exception ex)
            {
                ExportStatus = $"Erreur lors de la vérification de User : {ex.Message}";
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 2) Créer un nouvel utilisateur via l’API Auth
        // ──────────────────────────────────────────────────────────────
        private async Task OnCreateUser()
        {
            CreateError = "";
            CreateSuccess = "";

            if (string.IsNullOrWhiteSpace(NewUsername) ||
                string.IsNullOrWhiteSpace(NewPassword))
            {
                CreateError = "Nom et mot de passe requis.";
                return;
            }

            var ok = await _authService.RegisterAsync(
                NewUsername.Trim(),
                NewPassword,
                NewRole
            );

            if (!ok)
            {
                CreateError = $"L’utilisateur « {NewUsername} » existe déjà.";
                return;
            }

            CreateSuccess = $"Utilisateur « {NewUsername} » créé avec succès !";
            NewUsername = NewPassword = "";
            NewRole = UserRole.Technician;

            await LoadAllUsersAsync();
        }

        // ──────────────────────────────────────────────────────────────
        // 3) Charger les sessions de l’utilisateur sélectionné
        // ──────────────────────────────────────────────────────────────
        private async Task OnLoadSessionsByUser()
        {
            FilteredEntries.Clear();
            if (SelectedUser is null)
                return;

            try
            {
                var sessions = await _sessionService.GetTimeEntriesAsync(SelectedUser.Id);
                foreach (var s in sessions.OrderBy(s => s.StartTime))
                    FilteredEntries.Add(s);
            }
            catch (Exception ex)
            {
                ExportStatus = $"Erreur lors de la vérification de session : {ex.Message}";
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 4) Export CSV des sessions filtrées
        // ──────────────────────────────────────────────────────────────
        private async Task OnExportToCsv()
        {
            ExportStatus = "";

            if (SelectedUser is null)
            {
                ExportStatus = "Choisissez un utilisateur.";
                return;
            }

            if (!FilteredEntries.Any())
            {
                ExportStatus = "Aucune session à exporter.";
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Id,Type,Start,End,Duration,TravelTime,StartAddr,EndAddr,DinnerPaid");
                foreach (var s in FilteredEntries)
                {
                    var dur = s.WorkDuration.HasValue
                        ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m"
                        : "";
                    var travel = s.TravelTimeEstimate.HasValue
                        ? $"{s.TravelTimeEstimate:hh\\:mm}"
                        : "";

                    sb.AppendLine(string.Join(",",
                        s.Id,
                        s.SessionType,
                        s.StartTime.ToString("o"),
                        s.EndTime?.ToString("o") ?? "",
                        dur,
                        travel,
                        Escape(s.StartAddress),
                        Escape(s.EndAddress),
                        s.DinnerPaid
                    ));
                }

                var fn = $"sessions_{SelectedUser.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var path = Path.Combine(FileSystem.AppDataDirectory, fn);
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

                ExportStatus = $"Exporté : {fn}";
                await Share.RequestAsync(new ShareFileRequest(fn, new ShareFile(path)));
            }
            catch (Exception ex)
            {
                ExportStatus = $"Erreur lors de l’export : {ex.Message}";
            }
        }

        static string Escape(string? f) =>
            string.IsNullOrEmpty(f)
                ? ""
                : (f.Contains(',')
                    ? $"\"{f.Replace("\"", "\"\"")}\""
                    : f);

        // ──────────────────────────────────────────────────────────────
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
