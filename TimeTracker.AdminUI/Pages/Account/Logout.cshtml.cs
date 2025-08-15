// TimeTracker.AdminUI/Pages/Account/Logout.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity; // IdentityConstants.ApplicationScheme
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TimeTracker.AdminUI.Pages.Account;

[AllowAnonymous] // on autorise l’accès à la page, même après déconnexion
public class LogoutModel : PageModel
{
    public string? InfoMessage { get; private set; }

    public void OnGet()
    {
        // Si on arrive en GET, on propose de se déconnecter si encore authentifié,
        // sinon on affiche juste la page (le bouton n’apparaîtra pas).
    }

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostAsync()
    {
        // Déconnexion du cookie Identity (schéma option B)
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

        // Supprime le JWT utilisé par le HttpClient vers l’API
        if (Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Cookies.Delete("jwt_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
        }

        // Rester sur la page et afficher un message de confirmation
        InfoMessage = " ";
        return Page();
    }
}
