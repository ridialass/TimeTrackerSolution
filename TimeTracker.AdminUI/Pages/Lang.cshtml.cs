using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TimeTracker.AdminUI.Pages
{
    public class LangModel : PageModel
    {
        public void OnGet()
        {
            // Cette page peut être utilisée pour gérer la langue de l'application.
            // Par exemple, vous pouvez rediriger vers une page de sélection de langue
            // ou simplement afficher la langue actuelle.

            // Pour l'instant, nous n'avons pas de logique spécifique ici.
            // Vous pouvez ajouter des fonctionnalités pour changer la langue,
            // par exemple en utilisant des cookies ou des sessions.
            string? culture = Request.Query["culture"];
            Console.WriteLine("La langue sélectionnée est : " + culture);
            if (culture != null)
            {
                // Ajouter la logique pour changer la langue ici
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1)
                    }
                );
            }
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";    
            Response.Redirect(returnUrl);
        }
    }
}
