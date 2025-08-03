using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<Errors> _localizer;

        public ProfileModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env, IStringLocalizer<Errors> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
            _localizer = localizer;
        }

        [BindProperty]
        [Required(ErrorMessage = nameof(Errors.RequiredEmail))]
        [EmailAddress(ErrorMessage = nameof(Errors.InvalidEmail))]
        public string EditableEmail { get; set; } = string.Empty;

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Town { get; set; } = string.Empty;

        [BindProperty]
        public string Country { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? ProfilePicture { get; set; }

        [BindProperty]
        public string? ProfilePictureUrl { get; set; }

        public string ProfileMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            var resp = await client.GetAsync("api/employees/me");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserProfileDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                EditableEmail = user.Email ?? string.Empty;
                FirstName = user.FirstName ?? string.Empty;
                LastName = user.LastName ?? string.Empty;
                Town = user.Town ?? string.Empty;
                Country = user.Country ?? string.Empty;
                // Ici, on NE met jamais "/images/default-profile.png" dans la propriété du modèle
                ProfilePictureUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? null : user.ProfilePictureUrl;
            }
            else
            {
                ProfileMessage = _localizer["ProfileLoadError"];
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

            // On récupère la valeur *actuelle* postée (champ caché du formulaire)
            string? pictureUrl = ProfilePictureUrl;

            // S'il y a upload d'une nouvelle image, on l'utilise
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var ext = Path.GetExtension(ProfilePicture.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ProfileMessage = _localizer["InvalidPictureFormat"];
                    return Page();
                }
                if (ProfilePicture.Length > 2 * 1024 * 1024)
                {
                    ProfileMessage = _localizer["PictureTooLarge"];
                    return Page();
                }
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);
                var filename = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, filename);

                using (var stream = System.IO.File.Create(filePath))
                    await ProfilePicture.CopyToAsync(stream);

                pictureUrl = $"/uploads/avatars/{filename}";
            }

            // On n'écrase jamais par l'image par défaut ici : on laisse la valeur nulle ou l'URL existante
            // Si pictureUrl est null ou vide, le backend n'écrasera pas la valeur existante grâce à ta méthode UpdateMyProfileAsync

            var payload = new
            {
                Email = EditableEmail,
                FirstName,
                LastName,
                Town,
                Country,
                ProfilePictureUrl = pictureUrl ?? "" // chaîne vide si rien à changer (le backend ne modifiera pas)
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PutAsync("api/employees/me", content);

            if (resp.IsSuccessStatusCode)
            {
                ProfileMessage = _localizer["ProfileUpdateSuccess"];
                ProfilePictureUrl = pictureUrl; // met à jour la valeur pour l'affichage, sinon reste l'existante
            }
            else
            {
                string apiMsg = await resp.Content.ReadAsStringAsync();
                ProfileMessage = $"{_localizer["ProfileUpdateError"]} {apiMsg}";
            }

            return Page();
        }
    }
}