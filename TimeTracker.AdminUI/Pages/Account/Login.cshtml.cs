using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity; // <-- nécessaire pour IdentityConstants.ApplicationScheme
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Net.Http.Json;
using System.Security.Claims;
using TimeTracker.AdminUI.Serialization; // si tu utilises JsonDefaults.Options
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Account;

public class LoginModel(IHttpClientFactory http, IStringLocalizer<SharedResource> S) : PageModel
{
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            Response.Redirect("/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = S["LoginEmptyError"];
            return Page();
        }

        var client = http.CreateClient("TimeTrackerAPI");

        var payload = new LoginRequestDto { Username = Username.Trim(), Password = Password };

        using var content = JsonContent.Create(payload /*, options: JsonDefaults.Options */);
        var resp = await client.PostAsync("api/auth/login", content);

        if (!resp.IsSuccessStatusCode)
        {
            ErrorMessage = S["LoginInvalidError"];
            return Page();
        }

        var login = await resp.Content.ReadFromJsonAsync<LoginResponseDto>(/* JsonDefaults.Options */);
        if (login is null || string.IsNullOrWhiteSpace(login.Token) || string.IsNullOrWhiteSpace(login.Username))
        {
            ErrorMessage = S["LoginServerError"];
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, login.Username!),
            new(ClaimTypes.Role, login.Role.ToString()),
            new(ClaimTypes.NameIdentifier, login.ApplicationUserId.ToString())
        };

        // 🔑 Signe avec le schéma Identity (cookie .AspNetCore.Identity.Application)
        var identity = new ClaimsIdentity(
            claims,
            IdentityConstants.ApplicationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                IsPersistent = true,
                AllowRefresh = true,
                RedirectUri = returnUrl
            });

        // JWT pour le HttpClient handler (API)
        Response.Cookies.Append("jwt_token", login.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return login.Role.ToString() == "Admin"
            ? RedirectToPage("/Admin/AdminDashboard")
            : RedirectToPage("/UserPage");
    }
}
