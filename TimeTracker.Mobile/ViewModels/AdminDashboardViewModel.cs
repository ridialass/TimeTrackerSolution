using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using TimeTracker.Models;
using TimeTracker.Services;
using static TimeTracker.Models.Enum;

namespace TimeTracker.ViewModels
{
    public class AdminDashboardViewModel : INotifyPropertyChanged
    {
        private readonly SessionService _sessionService = App.SessionService;
        private readonly AuthService _authService = App.AuthService;

        // ════ SECTION 1 : Creation d’utilisateur ════

        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public ObservableCollection<UserRole> Roles { get; }
            = new ObservableCollection<UserRole>(
                (UserRole[])System.Enum.GetValues(typeof(UserRole)));

        private UserRole _newSelectedRole = UserRole.Technician;
        public UserRole NewSelectedRole
        {
            get => _newSelectedRole;
            set => SetProperty(ref _newSelectedRole, value);
        }

        private string _createUserErrorMessage;
        public string CreateUserErrorMessage
        {
            get => _createUserErrorMessage;
            set { SetProperty(ref _createUserErrorMessage, value); OnPropertyChanged(nameof(HasCreateUserError)); }
        }
        public bool HasCreateUserError => !string.IsNullOrEmpty(CreateUserErrorMessage);

        private string _createUserSuccessMessage;
        public string CreateUserSuccessMessage
        {
            get => _createUserSuccessMessage;
            set { SetProperty(ref _createUserSuccessMessage, value); OnPropertyChanged(nameof(HasCreateUserSuccess)); }
        }
        public bool HasCreateUserSuccess => !string.IsNullOrEmpty(CreateUserSuccessMessage);

        public ICommand CreateUserCommand { get; }

        // ════ SECTION 2 : Filtrage des sessions par utilisateur ════

        public ObservableCollection<User> AllUsers { get; }
            = new ObservableCollection<User>();

        private User _selectedFilterUser;
        public User SelectedFilterUser
        {
            get => _selectedFilterUser;
            set => SetProperty(ref _selectedFilterUser, value);
        }

        public ICommand LoadSessionsByUserCommand { get; }

        public ObservableCollection<WorkSession> FilteredSessions { get; }
            = new ObservableCollection<WorkSession>();

        // ════ SECTION 3 : Exportation en CSV ════

        private string _exportStatusMessage;
        public string ExportStatusMessage
        {
            get => _exportStatusMessage;
            set { SetProperty(ref _exportStatusMessage, value); OnPropertyChanged(nameof(HasExportStatusMessage)); }
        }
        public bool HasExportStatusMessage => !string.IsNullOrEmpty(ExportStatusMessage);

        public ICommand ExportToCsvCommand { get; }

        public AdminDashboardViewModel()
        {
            CreateUserCommand = new Command(async () => await OnCreateUser());
            LoadSessionsByUserCommand = new Command(async () => await OnLoadSessionsByUser());
            ExportToCsvCommand = new Command(async () => await OnExportToCsv());

            // Charger la liste des utilisateurs dès le départ
            Task.Run(async () => await LoadAllUsersAsync());
        }

        // ───────────────────────────────────────────────────
        // 1) Méthode pour créer un utilisateur
        // ───────────────────────────────────────────────────
        private async Task OnCreateUser()
        {
            CreateUserErrorMessage = string.Empty;
            CreateUserSuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            {
                CreateUserErrorMessage = "Username and Password cannot be empty.";
                return;
            }

            // Appeler RegisterAsync du service AuthService
            var success = await _authService.RegisterAsync(NewUsername.Trim(), NewPassword, NewSelectedRole);
            if (!success)
            {
                CreateUserErrorMessage = $"Username \"{NewUsername}\" already exists.";
                return;
            }

            CreateUserSuccessMessage = $"User \"{NewUsername}\" created successfully.";
            NewUsername = string.Empty;
            NewPassword = string.Empty;
            NewSelectedRole = UserRole.Technician;

            // Rafraîchir la liste des utilisateurs
            await LoadAllUsersAsync();
        }

        private async Task LoadAllUsersAsync()
        {
            AllUsers.Clear();
            var users = await App.DbContext.Connection.Table<User>().ToListAsync();
            foreach (var u in users.OrderBy(u => u.Username))
                AllUsers.Add(u);
        }

        // ───────────────────────────────────────────────────
        // 2) Méthode pour charger les sessions filtrées par utilisateur
        // ───────────────────────────────────────────────────
        private async Task OnLoadSessionsByUser()
        {
            if (SelectedFilterUser == null)
                return;

            FilteredSessions.Clear();
            var sessions = await _sessionService.GetSessionsByUserAsync(SelectedFilterUser.Id);
            foreach (var s in sessions)
                FilteredSessions.Add(s);
        }

        // ───────────────────────────────────────────────────
        // 3) Méthode pour exporter les sessions filtrées en CSV
        // ───────────────────────────────────────────────────
        private async Task OnExportToCsv()
        {
            ExportStatusMessage = string.Empty;

            if (SelectedFilterUser == null)
            {
                ExportStatusMessage = "No user selected for export.";
                return;
            }

            if (FilteredSessions.Count == 0)
            {
                ExportStatusMessage = "No sessions to export.";
                return;
            }

            try
            {
                // 1) Générer le contenu CSV
                var sb = new StringBuilder();
                // En‐tête
                sb.AppendLine("SessionId,Username,SessionType,StartTime,EndTime,WorkDuration,IncludesTravel,TravelTime,StartAddress,EndAddress,DinnerPaid");

                foreach (var s in FilteredSessions)
                {
                    var duration = s.WorkDuration.HasValue
                        ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m"
                        : "";
                    var travel = s.TravelTimeEstimate.HasValue
                        ? $"{s.TravelTimeEstimate.Value:hh\\:mm}"
                        : "";
                    var endTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("O") : "";
                    var endAddr = string.IsNullOrWhiteSpace(s.EndAddress) ? "" : s.EndAddress;

                    // Échapper les virgules dans les adresses en entourant par des guillemets
                    string Escape(string field)
                    {
                        if (string.IsNullOrEmpty(field)) return "";
                        if (field.Contains(",") || field.Contains("\""))
                            return "\"" + field.Replace("\"", "\"\"") + "\"";
                        return field;
                    }

                    sb.AppendLine(string.Join(",",
                        s.Id,
                        Escape(s.Username),
                        s.SessionType.ToString(),
                        s.StartTime.ToString("O"),
                        endTime,
                        Escape(duration),
                        s.IncludesTravelTime ? "Yes" : "No",
                        Escape(travel),
                        Escape(s.StartAddress),
                        Escape(endAddr),
                        s.DinnerPaid.ToString()
                    ));
                }

                // 2) Sauvegarder le fichier CSV dans AppDataDirectory
                var filename = $"sessions_{SelectedFilterUser.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, filename);
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                ExportStatusMessage = $"Export successful: {filename}";

                // 3) (Optionnel) proposer de partager ou ouvrir le fichier
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Exported Sessions",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                ExportStatusMessage = $"Export failed: {ex.Message}";
            }
        }

        // INotifyPropertyChanged …
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

        private void OnPropertyChanged([CallerMemberName] string propName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
