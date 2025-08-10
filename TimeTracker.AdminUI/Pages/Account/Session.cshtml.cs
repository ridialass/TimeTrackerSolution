// TimeTracker.AdminUI/Pages/Account/Session.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.AdminUI.Pages.Account;

[Authorize]
public class SessionModel : PageModel
{
    private readonly IHttpClientFactory _http;

    public SessionModel(IHttpClientFactory http) => _http = http;

    // Options JSON camelCase + case-insensitive
    private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public List<TimeEntryDto> Sessions { get; private set; } = new();

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
        await LoadSessionsAsync();
        return Page();
    }

    // si la vue poste avec asp-page-handler="LoadSessions"
    public async Task<IActionResult> OnPostLoadSessionsAsync()
    {
        await LoadSessionsAsync();
        return Page();
    }

    private async Task LoadSessionsAsync()
    {
        var client = _http.CreateClient("TimeTrackerAPI");

        var jwt = Request.Cookies["jwt_token"];
        if (!string.IsNullOrWhiteSpace(jwt))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // bornes UI
        var today = DateTime.Today;
        (CurrentWeekStart, CurrentWeekEnd) = GetWeekRange(today, WeekOffset);
        (CurrentMonthStart, CurrentMonthEnd) = GetMonthRange(today, MonthOffset);

        // 1) récupérer l'ID depuis les claims (fiable et immédiat)
        int userId = 0;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(idClaim))
            int.TryParse(idClaim, out userId);

        // 2) fallback via /me si besoin
        if (userId <= 0)
        {
            using var respMe = await client.GetAsync("api/employees/me");
            if (!respMe.IsSuccessStatusCode) { Sessions = new(); return; }

            var me = await respMe.Content.ReadFromJsonAsync<EmployeeDto>(JsonOpt);
            userId = me?.Id ?? 0;
            if (userId <= 0) { Sessions = new(); return; }
        }

        // 3) récupérer les sessions
        using var resp = await client.GetAsync($"api/timeentries?userId={userId}");
        if (resp.StatusCode == HttpStatusCode.NotFound) { Sessions = new(); return; }
        resp.EnsureSuccessStatusCode();

        var all = await resp.Content.ReadFromJsonAsync<List<TimeEntryDto>>(JsonOpt) ?? new();

        // 4) appliquer exactement UN filtre
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
                                      e.StartTime.Date <= CurrentWeekEnd)
        };

        // 5) dédupe : garder l'enregistrement le plus "complet"
        Sessions = filtered
            .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
            .Select(g => g.OrderByDescending(s => s.EndTime.HasValue)
                          .ThenByDescending(s => s.EndTime)
                          .First())
            .ToList();
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
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }
}
