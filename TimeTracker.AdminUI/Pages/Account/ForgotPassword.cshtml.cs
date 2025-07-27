using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources; // Pour la localisation des erreurs

namespace TimeTracker.AdminUI.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Errors> _localizer;

        public ForgotPasswordModel(IHttpClientFactory httpClientFactory, IStringLocalizer<Errors> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
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
                // Message localisé
                Message = _localizer["ForgotPasswordSuccess"];
            }
            else
            {
                Message = _localizer["ForgotPasswordError"];
            }

            return Page();
        }
    }
}
