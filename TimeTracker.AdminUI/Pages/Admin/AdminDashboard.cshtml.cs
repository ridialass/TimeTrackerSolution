// TimeTracker.AdminUI/Pages/Admin/AdminDashboard.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using TimeTracker.AdminUI.Serialization; // JsonDefaults.Options
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel(IHttpClientFactory http, IStringLocalizer<Errors> localizer) : PageModel
{
    public List<EmployeeDto> AllEmployees { get; set; } = [];
    [BindProperty(SupportsGet = true)] public int SelectedEmployeeId { get; set; }
    public List<TimeEntryDto> FilteredEntries { get; set; } = [];
    [BindProperty] public EmployeeDto NewUser { get; set; } = new() { Role = UserRole.Technician };
    [BindProperty] public string NewUserPassword { get; set; } = string.Empty;
    [BindProperty(SupportsGet = true)] public string SelectedPeriod { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public DateTime? CustomStartDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? CustomEndDate { get; set; }
    [BindProperty(SupportsGet = true)] public int WeekOffset { get; set; } = 0;

    public DateTime CurrentWeekStart { get; set; }
    public DateTime CurrentWeekEnd { get; set; }
    public string? CreateError { get; set; }
    public string? CreateSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity!.IsAuthenticated) return RedirectToPage("/Account/Login");
        if (!User.IsInRole("Admin")) return RedirectToPage("/Account/AccessDenied");

        await LoadEmployeesAsync();
        await LoadSessionsAsync();
        return Page();
    }

    private async Task LoadEmployeesAsync()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("api/employees");

        if (!response.IsSuccessStatusCode)
        {
            ViewData["ApiError"] = "Connexion expirée ou non autorisée. Veuillez vous reconnecter.";
            return;
        }

        AllEmployees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>(JsonDefaults.Options) ?? [];
    }

    private async Task LoadSessionsAsync()
    {
        if (SelectedEmployeeId == 0) { FilteredEntries = []; return; }

        var client = http.CreateClient("TimeTrackerAPI");
        var resp = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");
        if (resp.StatusCode == HttpStatusCode.NotFound) { FilteredEntries = []; return; }
        resp.EnsureSuccessStatusCode();

        // Lecture JSON avec options mises en cache (pas de source-gen ici)
        var entries = await resp.Content.ReadFromJsonAsync<List<TimeEntryDto>>(JsonDefaults.Options);
        FilteredEntries = entries ?? [];
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        CreateError = CreateSuccess = null;

        if (string.IsNullOrWhiteSpace(NewUser.Username) || string.IsNullOrWhiteSpace(NewUserPassword))
        {
            CreateError = localizer["CreateUserEmptyError"];
            await LoadEmployeesAsync();
            return Page();
        }

        var client = CreateAuthenticatedClient();
        using var content = JsonContent.Create(NewUser, options: JsonDefaults.Options);
        var response = await client.PostAsync($"api/auth/register?password={Uri.EscapeDataString(NewUserPassword)}", content);

        if (response.IsSuccessStatusCode)
            CreateSuccess = localizer["CreateUserSuccess", NewUser.Username];
        else if (response.StatusCode == HttpStatusCode.Conflict)
            CreateError = localizer["CreateUserConflictError", NewUser.Username];
        else
            CreateError = localizer["CreateUserGenericError"];

        NewUser = new EmployeeDto { Role = UserRole.Technician };
        NewUserPassword = string.Empty;
        await LoadEmployeesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostLoadSessionsAsync()
    {
        await LoadEmployeesAsync();
        await LoadSessionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostExportCsvAsync(
        int SelectedEmployeeId,
        string SelectedPeriod,
        DateTime? CustomStartDate,
        DateTime? CustomEndDate)
    {
        if (SelectedEmployeeId == 0)
        {
            ModelState.AddModelError(string.Empty, localizer["ExportNoEmployeeError"]);
            await LoadEmployeesAsync();
            return Page();
        }

        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, localizer["ExportSessionApiError"]);
            await LoadEmployeesAsync();
            return Page();
        }

        var allEntries = await response.Content.ReadFromJsonAsync<List<TimeEntryDto>>(JsonDefaults.Options) ?? [];

        var today = DateTime.Today;
        IEnumerable<TimeEntryDto> filtered = allEntries;

        if (SelectedPeriod == "month")
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            filtered = filtered.Where(e => e.StartTime.Date >= monthStart && e.StartTime.Date <= today);
        }
        else if (SelectedPeriod == "week")
        {
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff);
            var weekEnd = weekStart.AddDays(6);
            filtered = filtered.Where(e => e.StartTime.Date >= weekStart && e.StartTime.Date <= weekEnd);
        }
        else if (SelectedPeriod == "custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
        {
            filtered = filtered.Where(e => e.StartTime.Date >= CustomStartDate.Value && e.StartTime.Date <= CustomEndDate.Value);
        }

        var exportEntries = filtered
            .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
            .Select(g => g
                .OrderByDescending(s => s.EndTime.HasValue)
                .ThenByDescending(s => s.EndTime)
                .First())
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",",
            localizer["CsvId"],
            localizer["CsvUsername"],
            localizer["CsvSessionType"],
            localizer["CsvStartTime"],
            localizer["CsvEndTime"],
            localizer["CsvWorkDuration"],
            localizer["CsvIncludesTravel"],
            localizer["CsvTravelTime"],
            localizer["CsvStartAddress"],
            localizer["CsvEndAddress"],
            localizer["CsvDinnerPaid"]
        ));

        static string Escape(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            var needsQuotes = field.Contains(',') || field.Contains('"'); // char overloads
            if (!needsQuotes) return field;
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        foreach (var s in exportEntries)
        {
            var duration = s.WorkDuration != null ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m" : "";
            var travel = s.TravelTimeEstimate != null ? $"{s.TravelTimeEstimate.Value:hh\\:mm}" : "";
            var endTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("O") : "";
            var endAddr = string.IsNullOrWhiteSpace(s.EndAddress) ? "" : s.EndAddress;

            var sessionTypeValue = localizer[$"SessionType_{s.SessionType}"];
            var includesTravelValue = localizer[s.IncludesTravelTime ? "YesLabel" : "NoLabel"];
            var dinnerPaidValue = localizer[$"DinnerPaidBy_{s.DinnerPaid}"];

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
        var client = http.CreateClient("TimeTrackerAPI");
        if (Request.Cookies.TryGetValue("jwt_token", out var jwt) && !string.IsNullOrEmpty(jwt))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }
}
