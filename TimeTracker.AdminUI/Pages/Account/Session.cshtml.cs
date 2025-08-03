using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Net.Http.Headers;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Account
{
    [Authorize]
    public class SessionModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Errors> _localizer;

        public SessionModel(IHttpClientFactory httpClientFactory, IStringLocalizer<Errors> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        public List<TimeEntryDto> Sessions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SelectedPeriod { get; set; } = "week";
        [BindProperty(SupportsGet = true)]
        public DateTime? CustomStartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? CustomEndDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int WeekOffset { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public int MonthOffset { get; set; } = 0;

        public DateTime CurrentWeekStart { get; set; }
        public DateTime CurrentWeekEnd { get; set; }
        public DateTime CurrentMonthStart { get; set; }
        public DateTime CurrentMonthEnd { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Account/Login");

            await LoadSessionsAsync();
            return Page();
        }

        private async Task LoadSessionsAsync()
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            // 1. Récupérer l'utilisateur courant
            var respMe = await client.GetAsync("api/employees/me");
            if (!respMe.IsSuccessStatusCode)
            {
                Sessions = new();
                return;
            }
            var meJson = await respMe.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<EmployeeDto>(meJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (me?.Id == null)
            {
                Sessions = new();
                return;
            }

            // 2. Récupérer les sessions de l'utilisateur
            var respSessions = await client.GetAsync($"api/timeentries?userId={me.Id}");
            if (!respSessions.IsSuccessStatusCode)
            {
                Sessions = new();
                return;
            }

            var json = await respSessions.Content.ReadAsStringAsync();
            var allEntries = JsonSerializer.Deserialize<List<TimeEntryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            DateTime today = DateTime.Today;
            IEnumerable<TimeEntryDto> filtered = allEntries;

            // Semaine
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var baseWeekStart = today.AddDays(-1 * diff);
            var weekStart = baseWeekStart.AddDays(7 * WeekOffset);
            var weekEnd = weekStart.AddDays(6);
            CurrentWeekStart = weekStart;
            CurrentWeekEnd = weekEnd;

            // Mois
            var monthDate = today.AddMonths(MonthOffset);
            var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            CurrentMonthStart = monthStart;
            CurrentMonthEnd = monthEnd;

            // Filtrage selon la période sélectionnée
            if (SelectedPeriod == "month")
            {
                filtered = filtered.Where(e => e.StartTime.Date >= monthStart && e.StartTime.Date <= monthEnd);
            }
            else if (SelectedPeriod == "custom" && CustomStartDate.HasValue && CustomEndDate.HasValue)
            {
                filtered = filtered.Where(e => e.StartTime.Date >= CustomStartDate.Value && e.StartTime.Date <= CustomEndDate.Value);
            }
            else // semaine par défaut
            {
                filtered = filtered.Where(e => e.StartTime.Date >= weekStart && e.StartTime.Date <= weekEnd);
            }

            Sessions = filtered
                .GroupBy(e => new { e.StartTime, e.Username, e.StartAddress })
                .Select(g => g.OrderByDescending(s => s.EndTime.HasValue).ThenByDescending(s => s.EndTime).First())
                .ToList();
        }
    }
}