using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeTracker.Core.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DetailsTimeEntryModel : PageModel
    {
        public TimeEntryDto? TimeEntry { get; set; }

        // Filter properties...
        public int EmployeeId { get; set; }
        public string Period { get; set; } = string.Empty;
        public string CustomStartDate { get; set; } = string.Empty;
        public string CustomEndDate { get; set; } = string.Empty;
        public int WeekOffset { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsTimeEntryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(
            int id,
            int employeeId,
            string? period,
            string? customStartDate,
            string? customEndDate,
            int weekOffset)
        {
            EmployeeId = employeeId;
            Period = period ?? string.Empty;
            CustomStartDate = customStartDate ?? string.Empty;
            CustomEndDate = customEndDate ?? string.Empty;
            WeekOffset = weekOffset;

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
                System.Diagnostics.Debug.WriteLine(responseString); // <-- Add this
                // DEBUG: log responseString to see what's returned!
                var timeEntry = JsonSerializer.Deserialize<TimeEntryDto>(
                    responseString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (timeEntry != null)
                {
                    TimeEntry = timeEntry;
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
    }
}