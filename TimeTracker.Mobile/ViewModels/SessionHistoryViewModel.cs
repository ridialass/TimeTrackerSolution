using System.Collections.ObjectModel;
using System.ComponentModel;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class SessionHistoryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WorkSession> WorkSessions { get; set; } = new();

        public SessionHistoryViewModel()
        {
            LoadSessions();
        }

        private async void LoadSessions()
        {
            var service = App.SessionService;
            var sessions = await service.GetAllSessionsAsync();

            WorkSessions.Clear();
            foreach (var s in sessions)
                WorkSessions.Add(s);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
