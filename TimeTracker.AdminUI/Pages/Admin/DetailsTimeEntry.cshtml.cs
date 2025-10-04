using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DetailsTimeEntryModel : PageModel
    {
        private readonly IStringLocalizer<Errors> _Localizer;
        private readonly IHttpClientFactory _http;

        public DetailsTimeEntryModel(IHttpClientFactory http) => _http = http;

        public TimeEntryDto? TimeEntry { get; private set; }

        // 🔹 Chaîne d’affichage “hh:mm” (ou “—”)
        public string TravelDurationDisplay { get; private set; } = "—";

        // Filtres pour le bouton "Retour"
        [BindProperty(SupportsGet = true)] public int EmployeeId { get; set; }
        [BindProperty(SupportsGet = true)] public string? Period { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? CustomStartDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? CustomEndDate { get; set; }
        [BindProperty(SupportsGet = true)] public int WeekOffset { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _http.CreateClient("TimeTrackerAPI");

            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrWhiteSpace(jwt))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            else
                return RedirectToPage("/Account/Login");

            using var resp = await client.GetAsync($"api/timeentries/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToPage("/Account/Login");

                ModelState.AddModelError(string.Empty, "Erreur lors du chargement du pointage.");
                return Page();
            }

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = await resp.Content.ReadFromJsonAsync<TimeEntryDto>(opts);
            if (dto is null)
            {
                ModelState.AddModelError(string.Empty, "Impossible de charger le pointage.");
                return Page();
            }

            // Normalisation adresses
            dto.StartAddress = string.IsNullOrWhiteSpace(dto.StartAddress) ? null : dto.StartAddress.Trim();
            dto.EndAddress = string.IsNullOrWhiteSpace(dto.EndAddress) ? null : dto.EndAddress.Trim();

            // Fallback adresses via liste (si besoin)
            if ((string.IsNullOrWhiteSpace(dto.StartAddress) || string.IsNullOrWhiteSpace(dto.EndAddress)) && EmployeeId > 0)
            {
                using var respList = await client.GetAsync($"api/timeentries?userId={EmployeeId}");
                if (respList.IsSuccessStatusCode)
                {
                    var list = await respList.Content.ReadFromJsonAsync<List<TimeEntryDto>>(opts) ?? new();
                    var same = list.FirstOrDefault(e => e.Id == id);
                    if (same != null)
                    {
                        dto.StartAddress ??= string.IsNullOrWhiteSpace(same.StartAddress) ? null : same.StartAddress.Trim();
                        dto.EndAddress ??= string.IsNullOrWhiteSpace(same.EndAddress) ? null : same.EndAddress.Trim();
                    }
                }
            }

            TimeEntry = dto;

            // 🔹 Calcule la chaîne d’affichage de la durée trajet
            TravelDurationDisplay = FormatTravel(dto);

            return Page();
        }

        private static string FormatTravel(TimeEntryDto dto)
        {
            if (dto.TravelTimeEstimate.HasValue && dto.TravelTimeEstimate.Value > TimeSpan.Zero)
            {
                var ts = dto.TravelTimeEstimate.Value;
                return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}";
            }
            if (dto.TravelDurationHours.HasValue && dto.TravelDurationHours.Value > 0)
            {
                var minutes = (int)Math.Round(dto.TravelDurationHours.Value * 60.0, 0, MidpointRounding.AwayFromZero);
                var ts = TimeSpan.FromMinutes(minutes);
                return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}";
            }
            return "—";
        }
    }
}
