using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateTimeEntryModel : PageModel
    {
        [BindProperty]
        public TimeEntryDto TimeEntry { get; set; } = new();

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("OnPostAsync a été déclenché côté client web.");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Les erreurs de ModelState sont les suivantes :");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Clé : {error.Key}");
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Erreur : {subError.ErrorMessage}");
                    }
                }
                ModelState.AddModelError(string.Empty, "Le formulaire contient des erreurs. Veuillez corriger les champs.");
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("TimeTrackerAPI");

                // Get the JWT token from cookie and set as Authorization header
                var jwtToken = Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", jwtToken);
                }
                else
                {
                    // If no token, redirect to login page or show error
                    ModelState.AddModelError(string.Empty, "Session expirée ou non authentifiée. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }

                var jsonContent = JsonSerializer.Serialize(TimeEntry);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"Données envoyées : {jsonContent}");
                var response = await client.PostAsync("api/TimeEntries", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Statut de la réponse : {response.StatusCode}");
                Console.WriteLine($"Contenu de la réponse : {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Entrée ajoutée avec succès.");
                    return RedirectToPage("/Admin/Index", new { SelectedEmployeeId = TimeEntry.UserId });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token expired or not valid, redirect to login
                    ModelState.AddModelError(string.Empty, "Votre session a expiré. Veuillez vous reconnecter.");
                    return RedirectToPage("/Account/Login");
                }
                else
                {
                    Console.WriteLine($"Erreur API : {response.StatusCode}");
                    ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur inattendue : {ex.Message}");
                ModelState.AddModelError(string.Empty, $"Erreur inattendue : {ex.Message}");
                return Page();
            }
        }
    }
}