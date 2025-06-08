using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.AdminUI.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty, Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Exemple d’appel à l’API pour demander un reset
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var payload = new { email = Email.Trim() };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("api/auth/forgot-password", content);
            if (resp.IsSuccessStatusCode)
            {
                Message = "Si cette adresse existe, un mail vous sera envoyé.";
            }
            else
            {
                Message = "Une erreur est survenue, réessayez plus tard.";
            }

            return Page();
        }
    }
}
