using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditTimeEntryModel : PageModel
    {
        [BindProperty]
        public TimeEntryDto TimeEntry { get; set; } = new();

        // Filter/navigation context properties for page return
        [BindProperty(SupportsGet = true)] public int EmployeeId { get; set; }
        [BindProperty(SupportsGet = true)] public string Period { get; set; } = "all";
        [BindProperty] public int PauseHours { get; set; }
        [BindProperty] public int PauseMinutes { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public EditTimeEntryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: load entry to edit
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Debug.WriteLine($"[OnGetAsync] id param: {id}");
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwtToken = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Session expirée ou non authentifiée. Veuillez vous reconnecter.");
                return RedirectToPage("/Account/Login");
            }

            var response = await client.GetAsync($"api/TimeEntries/{id}");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var entry = JsonSerializer.Deserialize<TimeEntryDto>(
                    responseString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (entry != null)
                {
                    Debug.WriteLine($"[OnGetAsync] Loaded TimeEntry: {JsonSerializer.Serialize(entry)}");
                    TimeEntry = entry;
                    // Pré-calcul pour l'affichage rapide
                    var totalPause = entry.Pauses?
                        .Where(p => p.End.HasValue)
                        .Aggregate(TimeSpan.Zero, (acc, p) => acc + (p.End.Value - p.Start))
                        ?? TimeSpan.Zero;
                    PauseHours = totalPause.Hours + 24 * (totalPause.Days); // si > 24h, rare
                    PauseMinutes = totalPause.Minutes;
                    ViewData["PauseHours"] = PauseHours;
                    ViewData["PauseMinutes"] = PauseMinutes;
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Impossible de charger le pointage.");
                    return Page();
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(string.Empty, "Votre session a expiré. Veuillez vous reconnecter.");
                return RedirectToPage("/Account/Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Erreur lors du chargement du pointage.");
                return Page();
            }
        }

        // POST: save changes
        public async Task<IActionResult> OnPostAsync()
        {
            Debug.WriteLine($"[OnPostAsync] TimeEntry from POST: {JsonSerializer.Serialize(TimeEntry)}");
            Debug.WriteLine($"[OnPostAsync] PauseHours={PauseHours}, PauseMinutes={PauseMinutes}");
            Debug.WriteLine($"[OnPostAsync] ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Debug.WriteLine("[OnPostAsync] ModelState errors:");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Count > 0)
                    {
                        Debug.WriteLine($"  {kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                ModelState.AddModelError(string.Empty, "Le formulaire contient des erreurs. Veuillez corriger les champs.");
                return Page();
            }

            // Ajout pauses rapides (heure/minute) = écrase la liste si renseigné
            var totalPause = System.TimeSpan.FromHours(PauseHours) + System.TimeSpan.FromMinutes(PauseMinutes);
            if (totalPause.TotalMinutes > 0)
            {
                var workStart = TimeEntry.StartTime;
                var workEnd = TimeEntry.EndTime ?? TimeEntry.StartTime.AddHours(8);
                var sessionDuration = workEnd - workStart;
                // Place la pause au "milieu" de la session
                var pauseStart = workStart + System.TimeSpan.FromTicks(sessionDuration.Ticks / 2) - System.TimeSpan.FromTicks(totalPause.Ticks / 2);
                var pauseEnd = pauseStart + totalPause;
                TimeEntry.Pauses = new List<PausePeriodDto>
                {
                    new PausePeriodDto { Start = pauseStart, End = pauseEnd }
                };
                Debug.WriteLine($"[OnPostAsync] Pause générée : {pauseStart:HH:mm} - {pauseEnd:HH:mm}");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
                var jwtToken = Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", jwtToken);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Session expirée ou non authentifiée. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }

                var jsonContent = JsonSerializer.Serialize(TimeEntry);
                Debug.WriteLine($"[OnPostAsync] JSON envoyé à l'API: {jsonContent}");
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/TimeEntries/{TimeEntry.Id}", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[OnPostAsync] API Response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Admin/Index", new
                    {
                        SelectedEmployeeId = EmployeeId,
                        SelectedPeriod = Period
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(string.Empty, "Votre session a expiré. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Erreur lors de la modification.");
                    return Page();
                }
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erreur inattendue : {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            System.Console.WriteLine("Edit form submitted!");
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwtToken = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Session expirée ou non authentifiée. Veuillez vous reconnecter.");
                return RedirectToPage("/Account/Login");
            }

            var response = await client.DeleteAsync($"api/TimeEntries/{TimeEntry.Id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Admin/Index", new
                {
                    SelectedEmployeeId = EmployeeId,
                    SelectedPeriod = Period
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(string.Empty, "Votre session a expiré. Veuillez vous reconnecter.");
                return RedirectToPage("/Account/Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de la suppression.");
                return Page();
            }
        }
    }
}

