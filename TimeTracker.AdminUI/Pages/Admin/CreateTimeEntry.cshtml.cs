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
    public class CreateTimeEntryModel : PageModel
    {
        [BindProperty]
        public TimeEntryDto TimeEntry { get; set; } = new();

        public IActionResult OnGet(int userId)
        {
            TimeEntry.UserId = userId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Appeler l'API pour créer le pointage
            // TODO: Ajouter l'appel à l'API ici

            return RedirectToPage("/Admin/Index", new { SelectedEmployeeId = TimeEntry.UserId });
        }
    }
}