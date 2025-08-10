using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateTimeEntryModel : PageModel
    {
        [BindProperty]
        public TimeEntryDto TimeEntry { get; set; } = new();
        [BindProperty]
        public int PauseHours { get; set; }
        [BindProperty]
        public int PauseMinutes { get; set; }
        // Filter/navigation context properties for page return
        [BindProperty(SupportsGet = true)] public int SelectedEmployeeId { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public CreateTimeEntryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult OnGet(int employeeId, string employeeUsername)
        {
            TimeEntry.UserId = employeeId;
            TimeEntry.Username = employeeUsername ?? string.Empty;
            TimeEntry.StartTime = DateTime.Now;
            TimeEntry.EndTime = DateTime.Now.AddHours(8);
            TimeEntry.SessionType = WorkSessionType.Regular; // Set a default
            TimeEntry.DinnerPaid = DinnerPaidBy.None;         // Set a default
            TimeEntry.StartAddress = "";
            TimeEntry.EndAddress = "";
            TimeEntry.IncludesTravelTime = false;
            TimeEntry.TravelDurationHours = 0;
            // INIT champs pause pour le formulaire
            PauseHours = 0;
            PauseMinutes = 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Le formulaire contient des erreurs. Veuillez corriger les champs.");
                return Page();
            }

            // Ajoute la pause dans le DTO web si saisie > 0
            var totalPause = TimeSpan.FromHours(PauseHours) + TimeSpan.FromMinutes(PauseMinutes);
            if (totalPause.TotalMinutes > 0)
            {
                // Place la pause au "milieu" de la session pour la cohérence (optionnel)
                var workStart = TimeEntry.StartTime;
                var workEnd = TimeEntry.EndTime ?? TimeEntry.StartTime.AddHours(8);
                var sessionDuration = workEnd - workStart;
                var pauseStart = workStart + TimeSpan.FromTicks(sessionDuration.Ticks / 2) - TimeSpan.FromTicks(totalPause.Ticks / 2);
                var pauseEnd = pauseStart + totalPause;
                // Nettoyage possible des anciennes pauses (si tu veux écraser)
                TimeEntry.Pauses = new System.Collections.Generic.List<PausePeriodDto>
                {
                    new PausePeriodDto { Start = pauseStart, End = pauseEnd }
                };
            }
            else
            {
                TimeEntry.Pauses = new System.Collections.Generic.List<PausePeriodDto>();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
                var jwtToken = Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Session expirée ou non authentifiée. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }

                var jsonContent = JsonSerializer.Serialize(TimeEntry);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/TimeEntries", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Admin/Index", new { SelectedEmployeeId = TimeEntry.UserId });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Votre session a expiré. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur inattendue : {ex.Message}");
                return Page();
            }
        }
    }
}