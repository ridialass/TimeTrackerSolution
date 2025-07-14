using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TimeTracker.AdminUI.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public bool IsAdmin { get; set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            IsAdmin = User.IsInRole("Admin");
            if (IsAdmin)
            {
                // Redirige un admin vers l’UI admin
                return RedirectToPage("/Admin/Index");
            }

            // Sinon, affiche la homepage utilisateur
            return Page();
        }
    }
}

