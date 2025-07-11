using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using System.Threading.Tasks;
using System;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditTimeEntryModel : PageModel
    {
        [BindProperty]
        public TimeEntryDto TimeEntry { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Appeler l'API pour charger le pointage existant
            // TODO: Ajouter l'appel à l'API ici

            // Exemple :
            // TimeEntry = await ...;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Appeler l'API pour modifier le pointage
            // TODO: Ajouter l'appel à l'API ici

            return RedirectToPage("/Admin/Index", new { SelectedEmployeeId = TimeEntry.UserId });
        }
    }
}