using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace TimeTracker.AdminUI.Pages
{
    public class Index1Model : PageModel
    {


        // Optionnel : auto-rediriger si déjà connecté (décommente si tu veux ça)
        /*
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
                return User.IsInRole("Admin")
                    ? RedirectToPage("/Admin/AdminDashboard")
                    : RedirectToPage("/UserPage");
            return Page();
        }
        */
        public void OnGet()
        {

        }
    }    
}
