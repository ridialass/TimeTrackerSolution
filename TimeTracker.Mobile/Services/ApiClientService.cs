using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public ApiClientService()
        {
            // Hardcode or load from resource: e.g. “https://your‐api‐address.com/”
            _baseUrl = "https://YOUR_API_URL_HERE/";
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        private void SetAuthHeader(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        public async Task<LoginResponseDto?> LoginAsync(string username, string password)
        {
            var request = new LoginRequestDto
            {
                Username = username,
                Password = password
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _client.PostAsync("api/auth/login", content);
            if (!resp.IsSuccessStatusCode) return null;

            var respString = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponseDto>(respString,
                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> RegisterAsync(EmployeeDto newEmployee, string password)
        {
            var json = JsonSerializer.Serialize(newEmployee);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync($"api/auth/register?password={Uri.EscapeDataString(password)}", content);
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<EmployeeDto>> GetAllEmployeesAsync(string jwtToken)
        {
            SetAuthHeader(jwtToken);
            var resp = await _client.GetAsync("api/employee");
            resp.EnsureSuccessStatusCode();
            var s = await resp.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<EmployeeDto>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return list ?? new List<EmployeeDto>();
        }

        public async Task<List<TimeEntryDto>> GetTimeEntriesByEmployeeAsync(int employeeId, string jwtToken)
        {
            SetAuthHeader(jwtToken);
            var resp = await _client.GetAsync($"api/timeentry/employee/{employeeId}");
            resp.EnsureSuccessStatusCode();
            var s = await resp.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<TimeEntryDto>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return list ?? new List<TimeEntryDto>();
        }

        public async Task<TimeEntryDto> CreateTimeEntryAsync(TimeEntryDto payload, string jwtToken)
        {
            SetAuthHeader(jwtToken);
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("api/timeentry", content);
            resp.EnsureSuccessStatusCode();
            var s = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TimeEntryDto>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}
