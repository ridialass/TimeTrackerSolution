using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TimeTracker.AdminUI.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Supprime le cookie d'authentification
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // (optionnel) supprimez aussi votre jwt_token
            Response.Cookies.Delete("jwt_token");

            return RedirectToPage("/Account/Login");
        }
    }
}
