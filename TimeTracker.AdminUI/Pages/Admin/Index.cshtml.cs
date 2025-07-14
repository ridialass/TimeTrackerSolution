using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimeTracker.AdminUI.Resources;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<PagesTexts> _localizer;

        public List<EmployeeDto> AllEmployees { get; set; } = new();
        [BindProperty] public int SelectedEmployeeId { get; set; }
        public List<TimeEntryDto> FilteredEntries { get; set; } = new();
        [BindProperty] public EmployeeDto NewUser { get; set; } = new();
        [BindProperty] public string NewUserPassword { get; set; } = "";
        [BindProperty] public string SelectedPeriod { get; set; } = "all";
        [BindProperty] public DateTime? CustomStartDate { get; set; }
        [BindProperty] public DateTime? CustomEndDate { get; set; }
        [BindProperty] public int WeekOffset { get; set; } = 0;
        public DateTime CurrentWeekStart { get; set; }
        public DateTime CurrentWeekEnd { get; set; }
        public string? CreateError { get; set; }
        public string? CreateSuccess { get; set; }
        public List<TimeEntryDto> Sessions { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory, IStringLocalizer<TimeTracker.AdminUI.Resources.PagesTexts> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Account/Login");

            if (!User.IsInRole("Admin"))
                return RedirectToPage("/Account/AccessDenied");

            await LoadEmployeesAsync();
            return Page();
        
        }

        private async Task LoadEmployeesAsync()
        {
            var client = CreateAuthenticatedClient();
            var response = await client.GetAsync("api/employees");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ViewData["ApiError"] = "Connexion expirée ou non autorisée. Veuillez vous reconnecter.";
        return;
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
                CreateError = _localizer["CreateUserEmptyError"];
                await LoadEmployeesAsync();
                return Page();
            }

            var client = CreateAuthenticatedClient();
            var dtoJson = JsonSerializer.Serialize(NewUser);
            var content = new StringContent(dtoJson, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"api/auth/register?password={Uri.EscapeDataString(NewUserPassword)}", content);
            if (response.IsSuccessStatusCode)
            {
                CreateSuccess = _localizer["CreateUserSuccess", NewUser.Username];
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                CreateError = _localizer["CreateUserConflictError", NewUser.Username];
            }
            else
            {
                CreateError = _localizer["CreateUserGenericError"];
            }

            NewUser = new EmployeeDto { Role = UserRole.Technician };
            NewUserPassword = "";
            await LoadEmployeesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostLoadSessionsAsync()
        {
            if (SelectedEmployeeId == 0)
            {
                FilteredEntries = new List<TimeEntryDto>();
                await LoadEmployeesAsync();
                return Page();
            }

            var client = CreateAuthenticatedClient();
            var response = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                FilteredEntries = new List<TimeEntryDto>();
                await LoadEmployeesAsync();
                return Page();
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var allEntries = JsonSerializer.Deserialize<List<TimeEntryDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<TimeEntryDto>();

            DateTime today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var baseWeekStart = today.AddDays(-1 * diff);
            var weekStart = baseWeekStart.AddDays(7 * WeekOffset);
            var weekEnd = weekStart.AddDays(6);

            CurrentWeekStart = weekStart;
            CurrentWeekEnd = weekEnd;

            IEnumerable<TimeEntryDto> filtered = allEntries;

            if (SelectedPeriod == "month")
            {
                var monthStart = new DateTime(today.Year, today.Month, 1);
                filtered = filtered.Where(e => e.StartTime.Date >= monthStart && e.StartTime.Date <= today);
            }
            else if (SelectedPeriod == "custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
            {
                filtered = filtered.Where(e => e.StartTime.Date >= CustomStartDate.Value && e.StartTime.Date <= CustomEndDate.Value);
            }
            // else if week: already handled by week window

            filtered = filtered.Where(e => e.StartTime.Date >= weekStart && e.StartTime.Date <= weekEnd);

            FilteredEntries = filtered
                .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
                .Select(g =>
                    g.OrderByDescending(s => s.EndTime.HasValue)
                     .ThenByDescending(s => s.EndTime)
                     .First()
                )
                .ToList();

            await LoadEmployeesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostExportCsvAsync(
            int SelectedEmployeeId,
            string SelectedPeriod,
            DateTime? CustomStartDate,
            DateTime? CustomEndDate
        )
        {
            if (SelectedEmployeeId == 0)
            {
                ModelState.AddModelError("", _localizer["ExportNoEmployeeError"]);
                await LoadEmployeesAsync();
                return Page();
            }

            var client = CreateAuthenticatedClient();
            var response = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", _localizer["ExportSessionApiError"]);
                await LoadEmployeesAsync();
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var allEntries = JsonSerializer.Deserialize<List<TimeEntryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeEntryDto>();

            DateTime today = DateTime.Today;
            IEnumerable<TimeEntryDto> filtered = allEntries;

            if (SelectedPeriod == "month")
            {
                var monthStart = new DateTime(today.Year, today.Month, 1);
                filtered = filtered.Where(e => e.StartTime.Date >= monthStart && e.StartTime.Date <= today);
            }
            else if (SelectedPeriod == "week")
            {
                var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = today.AddDays(-1 * diff);
                var weekEnd = weekStart.AddDays(6);
                filtered = filtered.Where(e => e.StartTime.Date >= weekStart && e.StartTime.Date <= weekEnd);
            }
            else if (SelectedPeriod == "custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
            {
                filtered = filtered.Where(e => e.StartTime.Date >= CustomStartDate.Value && e.StartTime.Date <= CustomEndDate.Value);
            }
            // else: all

            var exportEntries = filtered
                .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
                .Select(g =>
                    g.OrderByDescending(s => s.EndTime.HasValue)
                     .ThenByDescending(s => s.EndTime)
                     .First()
                )
                .ToList();

            // Localized CSV columns
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",",
                _localizer["CsvId"],
                _localizer["CsvUsername"],
                _localizer["CsvSessionType"],
                _localizer["CsvStartTime"],
                _localizer["CsvEndTime"],
                _localizer["CsvWorkDuration"],
                _localizer["CsvIncludesTravel"],
                _localizer["CsvTravelTime"],
                _localizer["CsvStartAddress"],
                _localizer["CsvEndAddress"],
                _localizer["CsvDinnerPaid"]
            ));

            foreach (var s in exportEntries)
            {
                var duration = s.WorkDuration != null ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m" : "";
                var travel = s.TravelTimeEstimate != null ? $"{s.TravelTimeEstimate.Value:hh\\:mm}" : "";
                var endTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("O") : "";
                var endAddr = string.IsNullOrWhiteSpace(s.EndAddress) ? "" : s.EndAddress;
                string Escape(string field)
                {
                    if (string.IsNullOrEmpty(field)) return "";
                    if (field.Contains(",") || field.Contains("\""))
                        return "\"" + field.Replace("\"", "\"\"") + "\"";
                    return field;
                }
                // Localized values for SessionType and DinnerPaid
                string sessionTypeValue = _localizer[$"SessionType_{s.SessionType}"];
                string includesTravelValue = _localizer[s.IncludesTravelTime ? "YesLabel" : "NoLabel"];
                string dinnerPaidValue = _localizer[$"DinnerPaidBy_{s.DinnerPaid}"];

                sb.AppendLine(string.Join(",",
                    s.Id,
                    Escape(s.Username),
                    Escape(sessionTypeValue),
                    s.StartTime.ToString("O"),
                    endTime,
                    Escape(duration),
                    includesTravelValue,
                    Escape(travel),
                    Escape(s.StartAddress ?? ""),
                    Escape(endAddr),
                    dinnerPaidValue
                ));
            }

            var username = AllEmployees.FirstOrDefault(e => e.Id == SelectedEmployeeId)?.Username ?? "Unknown";
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"sessions_{username}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            }
            return client;
        }
    }
}