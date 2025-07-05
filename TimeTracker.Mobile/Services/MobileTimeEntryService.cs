using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public class MobileTimeEntryService : IMobileTimeEntryService
    {
        private readonly HttpClient _httpClient;
        private readonly ISessionStateService _sessionStateService;
        private TimeEntryDto? _inProgress;

        public MobileTimeEntryService(HttpClient httpClient, ISessionStateService sessionStateService)
        {
            _httpClient = httpClient;
            _sessionStateService = sessionStateService;
        }

        public TimeEntryDto? InProgressSession => _inProgress;

        // Call this when app starts or when a page appears to reload persisted state
        public async Task LoadInProgressSessionAsync()
        {
            _inProgress = await _sessionStateService.GetCurrentSessionAsync();
        }

        public async Task StartSessionAsync(TimeEntryDto dto)
        {
            _inProgress = dto;
            await _sessionStateService.SetCurrentSessionAsync(dto);
        }

        public async Task EndAndSaveCurrentSessionAsync()
        {
            if (_inProgress == null) return;
            await CreateTimeEntryAsync(_inProgress);
            _inProgress = null;
            await _sessionStateService.ClearSessionAsync();
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