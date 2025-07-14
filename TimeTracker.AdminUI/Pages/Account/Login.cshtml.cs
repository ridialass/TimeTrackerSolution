using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.AdminUI.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public string Username { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                Response.Redirect("/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Admin/Index");

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez saisir votre nom d’utilisateur et votre mot de passe.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var loginDto = new LoginRequestDto
            {
                Username = Username.Trim(),
                Password = Password
            };
            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // IMPORTANT : URL absolue !
            var response = await client.PostAsync("api/auth/login", content);
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Identifiants invalides.";
                return Page();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (loginResponse == null
                || string.IsNullOrWhiteSpace(loginResponse.Token)
                || string.IsNullOrWhiteSpace(loginResponse.Username) // on s'assure qu'il n'est pas null/empty
                )
            {
                ErrorMessage = "Réponse invalide du serveur.";
                return Page();
            }

            // Maintenant on peut utiliser l'opérateur ! en toute sécurité
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, loginResponse.Username!),
        new Claim(ClaimTypes.Role, loginResponse.Role.ToString()!)  // idem pour Role
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                IsPersistent = true,
                AllowRefresh = true,
                RedirectUri = returnUrl
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props
            );

            Response.Cookies.Append("jwt_token", loginResponse.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return LocalRedirect(returnUrl);
        }

    }
}
