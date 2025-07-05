using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Mobile.Models;

namespace TimeTracker.Mobile.Services
{
    public class MobileTimeEntryService : IMobileTimeEntryService
    {
        private readonly HttpClient _httpClient;
        private TimeEntryDto? _inProgress;
        // private readonly ITimeEntryRepository _timeEntryRepo;

        public MobileTimeEntryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public TimeEntryDto? InProgressSession => _inProgress;

        public Task StartSessionAsync(TimeEntryDto dto)
        {
            _inProgress = dto;
            return Task.CompletedTask;
        }

        public async Task EndAndSaveCurrentSessionAsync()
        {
            if (_inProgress == null) return;
            await CreateTimeEntryAsync(_inProgress);
            _inProgress = null;
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            var list = await _httpClient.GetFromJsonAsync<IEnumerable<TimeEntryDto>>(
                $"api/timeentries?userId={userId}");
            return list ?? Array.Empty<TimeEntryDto>();
        }

        public async Task CreateTimeEntryAsync(TimeEntryDto entry)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/timeentries", entry);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Échec création entrée: {response.StatusCode} - {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur: " + ex.Message);
                throw;
            }
        }

    }

}
