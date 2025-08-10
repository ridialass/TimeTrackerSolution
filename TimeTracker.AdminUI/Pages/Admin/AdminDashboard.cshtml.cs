// TimeTracker.AdminUI/Pages/Admin/AdminDashboard.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AdminDashboardModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly IStringLocalizer<Errors> _L;

    public AdminDashboardModel(IHttpClientFactory http, IStringLocalizer<Errors> localizer)
    {
        _http = http;
        _L = localizer;
    }

    // État / filtres
    public List<EmployeeDto> AllEmployees { get; private set; } = new();
    public List<TimeEntryDto> FilteredEntries { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public int SelectedEmployeeId { get; set; }
    [BindProperty(SupportsGet = true)] public string SelectedPeriod { get; set; } = "week"; // all | week | month | custom
    [BindProperty(SupportsGet = true)] public DateTime? CustomStartDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? CustomEndDate { get; set; }
    [BindProperty(SupportsGet = true)] public int WeekOffset { get; set; } = 0;
    [BindProperty(SupportsGet = true)] public int MonthOffset { get; set; } = 0;

    public DateTime CurrentWeekStart { get; private set; }
    public DateTime CurrentWeekEnd { get; private set; }
    public DateTime CurrentMonthStart { get; private set; }
    public DateTime CurrentMonthEnd { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadEmployeesAsync();
        await LoadSessionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostLoadSessionsAsync()
    {
        await LoadEmployeesAsync();
        await LoadSessionsAsync();
        return Page();
    }

    private async Task LoadEmployeesAsync()
    {
        var client = CreateAuthenticatedClient();
        using var resp = await client.GetAsync("api/employees");
        if (!resp.IsSuccessStatusCode) { AllEmployees = new(); return; }
        AllEmployees = await resp.Content.ReadFromJsonAsync<List<EmployeeDto>>() ?? new();
    }

    private async Task LoadSessionsAsync()
    {
        // Pose les bornes semaine & mois pour l’UI AVANT filtrage
        var today = DateTime.Today;
        (CurrentWeekStart, CurrentWeekEnd) = GetWeekRange(today, WeekOffset);
        (CurrentMonthStart, CurrentMonthEnd) = GetMonthRange(today, MonthOffset);

        if (SelectedEmployeeId == 0) { FilteredEntries = new(); return; }

        var client = CreateAuthenticatedClient();
        using var resp = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");
        if (resp.StatusCode == HttpStatusCode.NotFound) { FilteredEntries = new(); return; }
        resp.EnsureSuccessStatusCode();

        var all = await resp.Content.ReadFromJsonAsync<List<TimeEntryDto>>() ?? new();

        // ✅ Normalise et applique UN SEUL filtre
        var period = (SelectedPeriod ?? "week").Trim().ToLowerInvariant();

        IEnumerable<TimeEntryDto> filtered = period switch
        {
            "all" => all,
            "month" => all.Where(e => e.StartTime.Date >= CurrentMonthStart &&
                                      e.StartTime.Date <= CurrentMonthEnd),
            "custom" when CustomStartDate.HasValue && CustomEndDate.HasValue =>
                       all.Where(e => e.StartTime.Date >= CustomStartDate.Value.Date &&
                                      e.StartTime.Date <= CustomEndDate.Value.Date),
            _ => all.Where(e => e.StartTime.Date >= CurrentWeekStart &&
                                      e.StartTime.Date <= CurrentWeekEnd) // week par défaut
        };

        // Déduplication : conserve la ligne la plus “complète”
        FilteredEntries = filtered
            .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
            .Select(g => g.OrderByDescending(s => s.EndTime.HasValue).ThenByDescending(s => s.EndTime).First())
            .ToList();
    }

    public async Task<IActionResult> OnPostExportCsvAsync(
        int SelectedEmployeeId, string SelectedPeriod, DateTime? CustomStartDate, DateTime? CustomEndDate)
    {
        if (SelectedEmployeeId == 0)
        {
            ModelState.AddModelError("", _L["ExportNoEmployeeError"]);
            await LoadEmployeesAsync();
            return Page();
        }

        var client = CreateAuthenticatedClient();
        using var resp = await client.GetAsync($"api/timeentries?userId={SelectedEmployeeId}");
        if (!resp.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", _L["ExportSessionApiError"]);
            await LoadEmployeesAsync();
            return Page();
        }

        var all = await resp.Content.ReadFromJsonAsync<List<TimeEntryDto>>() ?? new();

        var today = DateTime.Today;
        var (wStart, wEnd) = GetWeekRange(today, WeekOffset);
        var (mStart, mEnd) = GetMonthRange(today, MonthOffset);

        var period = (SelectedPeriod ?? "week").Trim().ToLowerInvariant();
        IEnumerable<TimeEntryDto> filtered = period switch
        {
            "month" => all.Where(e => e.StartTime.Date >= mStart && e.StartTime.Date <= mEnd),
            "week" => all.Where(e => e.StartTime.Date >= wStart && e.StartTime.Date <= wEnd),
            "custom" when CustomStartDate.HasValue && CustomEndDate.HasValue =>
                       all.Where(e => e.StartTime.Date >= CustomStartDate.Value.Date &&
                                      e.StartTime.Date <= CustomEndDate.Value.Date),
            _ => all
        };

        var export = filtered
            .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
            .Select(g => g.OrderByDescending(s => s.EndTime.HasValue).ThenByDescending(s => s.EndTime).First())
            .ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",",
            _L["CsvId"], _L["CsvUsername"], _L["CsvSessionType"], _L["CsvStartTime"], _L["CsvEndTime"],
            _L["CsvWorkDuration"], _L["CsvIncludesTravel"], _L["CsvTravelTime"],
            _L["CsvStartAddress"], _L["CsvEndAddress"], _L["CsvDinnerPaid"]));

        foreach (var s in export)
        {
            static string Esc(string? f) =>
                string.IsNullOrEmpty(f) ? "" : (f.Contains(',') || f.Contains('"')) ? "\"" + f.Replace("\"", "\"\"") + "\"" : f;

            var duration = s.WorkDuration is not null ? $"{(int)s.WorkDuration.Value.TotalHours}h{s.WorkDuration.Value.Minutes}m" : "";
            var travel = s.TravelTimeEstimate is not null ? $"{s.TravelTimeEstimate.Value:hh\\:mm}" : "";
            var endTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("O") : "";
            var endAddr = string.IsNullOrWhiteSpace(s.EndAddress) ? "" : s.EndAddress;

            sb.AppendLine(string.Join(",",
                s.Id,
                Esc(s.Username),
                Esc(_L[$"SessionType_{s.SessionType}"]),
                s.StartTime.ToString("O"),
                endTime,
                Esc(duration),
                _L[s.IncludesTravelTime ? "YesLabel" : "NoLabel"],
                Esc(travel),
                Esc(s.StartAddress ?? ""),
                Esc(endAddr),
                _L[$"DinnerPaidBy_{s.DinnerPaid}"]));
        }

        var username = AllEmployees.FirstOrDefault(e => e.Id == SelectedEmployeeId)?.Username ?? "Unknown";
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"sessions_{username}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    // Helpers
    private HttpClient CreateAuthenticatedClient()
    {
        var c = _http.CreateClient("TimeTrackerAPI");
        var jwt = Request.Cookies["jwt_token"];
        if (!string.IsNullOrWhiteSpace(jwt))
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return c;
    }

    private static (DateTime start, DateTime end) GetWeekRange(DateTime today, int offsetWeeks)
    {
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var start = today.AddDays(-diff).AddDays(7 * offsetWeeks);
        return (start, start.AddDays(6));
    }

    private static (DateTime start, DateTime end) GetMonthRange(DateTime today, int offsetMonths)
    {
        var d = today.AddMonths(offsetMonths);
        var start = new DateTime(d.Year, d.Month, 1);
        return (start, start.AddMonths(1).AddDays(-1));
    }
}
