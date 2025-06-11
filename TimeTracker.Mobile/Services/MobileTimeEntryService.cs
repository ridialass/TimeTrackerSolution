// MobileTimeEntryService.cs
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public class MobileTimeEntryService : IMobileTimeEntryService
    {
        private readonly IApiClientService _apiClient;

        public MobileTimeEntryService(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            var response = await _apiClient.GetAsync($"api/timeentries?userId={userId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer
                .Deserialize<IEnumerable<TimeEntryDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? Array.Empty<TimeEntryDto>();
        }

        public async Task<bool> CreateTimeEntryAsync(TimeEntryDto entry)
        {
            var json = JsonSerializer.Serialize(entry);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/timeentries", content);
            return response.IsSuccessStatusCode;
        }
    }
}
