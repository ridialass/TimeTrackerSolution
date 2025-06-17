using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public List<EmployeeDto> AllEmployees { get; set; } = new();
        [BindProperty] public int SelectedEmployeeId { get; set; }
        public List<TimeEntryDto> FilteredEntries { get; set; } = new();
        [BindProperty] public EmployeeDto NewUser { get; set; } = new();
        [BindProperty] public string NewUserPassword { get; set; } = "";
        public string? CreateError { get; set; }
        public string? CreateSuccess { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;
        public List<TimeEntryDto> Sessions { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadEmployeesAsync();
            return Page();
        }

        private async Task LoadEmployeesAsync()
        {
            var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("api/employee");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Affiche l’erreur retournée par l’API dans ta page
                throw new Exception($"API Error {(int)response.StatusCode}:\n{body}");
            }

            AllEmployees = JsonSerializer.Deserialize<List<EmployeeDto>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<EmployeeDto>();
        }

        public async Task<IActionResult> OnPostCreateUserAsync()
        {
            CreateError = CreateSuccess = null;
            if (string.IsNullOrWhiteSpace(NewUser.Username) || string.IsNullOrWhiteSpace(NewUserPassword))
            {
                CreateError = "Username/password cannot be empty.";
                await LoadEmployeesAsync();
                return Page();
            }

            var client = CreateAuthenticatedClient();
            var dtoJson = JsonSerializer.Serialize(NewUser);
            var content = new StringContent(dtoJson, Encoding.UTF8, "application/json");
            // call: POST /api/auth/register?password={NewUserPassword}
            var response = await client.PostAsync($"api/auth/register?password={Uri.EscapeDataString(NewUserPassword)}", content);
            if (response.IsSuccessStatusCode)
            {
                CreateSuccess = $"User '{NewUser.Username}' created successfully.";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                CreateError = $"Username '{NewUser.Username}' already exists.";
            }
            else
            {
                CreateError = "Failed to create user.";
            }

            NewUser = new EmployeeDto { Role = UserRole.Technician };
            NewUserPassword = "";
            await LoadEmployeesAsync();
            return Page();
        }

        public async Task OnPostLoadSessionsAsync(int userId)
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var response = await client.GetAsync($"api/timeentries?userId={userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // pas de sessions pour cet utilisateur : on renvoie une liste vide
                Sessions = new List<TimeEntryDto>();
                return;
            }

            // pour toute autre erreur HTTP, on laisse passer l'exception
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Sessions = JsonSerializer.Deserialize<List<TimeEntryDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<TimeEntryDto>();
        }

        public IActionResult OnPostExportCsv()
        {
            if (FilteredEntries == null || !FilteredEntries.Any())
            {
                ModelState.AddModelError("", "No sessions to export.");
                return RedirectToPage();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Username,SessionType,StartTime,EndTime,WorkDuration,IncludesTravel,TravelTime,StartAddress,EndAddress,DinnerPaid");

            foreach (var s in FilteredEntries)
            {
                var duration = s.WorkDuration != null
                    ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m"
                    : "";
                var travel = s.TravelTimeEstimate != null
                    ? $"{s.TravelTimeEstimate.Value:hh\\:mm}"
                    : "";
                var endTime = s.EndTime.HasValue
                    ? s.EndTime.Value.ToString("O")
                    : "";
                var endAddr = string.IsNullOrWhiteSpace(s.EndAddress)
                    ? ""
                    : s.EndAddress;

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
                    Escape(s.StartAddress ?? ""),
                    Escape(endAddr),
                    s.DinnerPaid.ToString()
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"sessions_{AllEmployees.First(e => e.Id == SelectedEmployeeId).Username}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"]; // <== ici, récupération cookie sécurisé
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
            return client;
        }
    }
}
