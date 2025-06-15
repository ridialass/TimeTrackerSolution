using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

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
            var response = await _httpClient.PostAsJsonAsync("api/timeentries", entry);
            response.EnsureSuccessStatusCode();
        }

        //public async Task<TimeEntryDto> AddTimeEntryAsync(TimeEntryDto dto)
        //{
        //    var entity = _mapper.Map<TimeEntry>(dto);
        //    var saved = await _timeEntryRepo.AddAsync(entity);
        //    return _mapper.Map<TimeEntryDto>(saved);
        //}
    }

}
