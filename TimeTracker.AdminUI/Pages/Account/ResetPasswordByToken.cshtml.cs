using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TimeTracker.AdminUI.Pages.Account
{
    public class ResetPasswordByTokenModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetPasswordByTokenModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        [Required]
        public string Token { get; set; }

        [BindProperty(SupportsGet = true)]
        [Required]
        public string Email { get; set; }

        [BindProperty, Required, MinLength(6)]
        public string NewPassword { get; set; }

        [BindProperty, Required, Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            // Les propriétés Email et Token sont automatiquement renseignées depuis la query string
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var payload = new
            {
                email = Email?.Trim(),
                token = Token,
                newPassword = NewPassword
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("api/auth/reset-password", content);
            if (resp.IsSuccessStatusCode)
            {
                Message = "Votre mot de passe a été réinitialisé avec succès.";
            }
            else
            {
                // Tu peux lire le body pour un message d'erreur plus précis si besoin
                Message = "Erreur lors de la réinitialisation, le lien a peut-être expiré.";
            }

            return Page();
        }
    }
}