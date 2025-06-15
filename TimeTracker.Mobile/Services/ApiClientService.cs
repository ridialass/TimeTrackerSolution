// ApiClientService.cs
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _http;
        public ApiClientService(HttpClient httpClient) => _http = httpClient;

        public async Task<LoginResponseDto> LoginAsync(string u, string p)
        {
            var dto = new LoginRequestDto { Username = u, Password = p };
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);
            res.EnsureSuccessStatusCode();
            return (await res.Content.ReadFromJsonAsync<LoginResponseDto>())!;
        }

        public async Task RegisterAsync(RegisterRequestDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", dto);
            res.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<EmployeeDto>>("api/employees")
                   ?? Array.Empty<EmployeeDto>();
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            return await _http.GetFromJsonAsync<IEnumerable<TimeEntryDto>>($"api/timeentries?userId={userId}")
                   ?? Array.Empty<TimeEntryDto>();
        }

        public async Task CreateTimeEntryAsync(TimeEntryDto entry)
        {
            var res = await _http.PostAsJsonAsync("api/timeentries", entry);
            res.EnsureSuccessStatusCode();
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
            => _http.GetAsync(requestUri);

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
            => _http.PostAsync(requestUri, content);
    }
}
