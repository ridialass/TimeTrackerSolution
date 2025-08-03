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
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Errors> _localizer;

        public ChangePasswordModel(IHttpClientFactory httpClientFactory, IStringLocalizer<Errors> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        public string? Message { get; set; }
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Le mot de passe actuel est requis")]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = "";

            [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; } = "";

            [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
            public string ConfirmPassword { get; set; } = "";
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");

            // AJOUTER L'AUTHENTIFICATION ICI
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword
            };
            var content = new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("api/auth/change-password", content);
            if (resp.IsSuccessStatusCode)
            {
                SuccessMessage = _localizer["ChangePasswordSuccess"];
                ModelState.Clear();
                Input = new();
            }
            else
            {
                Message = _localizer["ChangePasswordError"];
            }

            return Page();
        }
    }
}