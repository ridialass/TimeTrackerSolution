// TimeTracker.AdminUI/Pages/Account/ChangePassword.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TimeTracker.AdminUI.Serialization;     // JsonDefaults.Options
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Resources;

namespace TimeTracker.AdminUI.Pages.Account;

[Authorize] // ← cookie Identity.Application requis (Option B)
public class ChangePasswordModel(IHttpClientFactory http, IStringLocalizer<Errors> L) : PageModel
{
    public string? Message { get; set; }
    public string? SuccessMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    [BindProperty] public InputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        // HttpClient configuré dans Program.cs avec JwtCookieAuthHandler :
        // le Bearer est pris du cookie "jwt_token" automatiquement.
        var client = http.CreateClient("TimeTrackerAPI");

        var dto = new ChangePasswordRequestDto
        {
            CurrentPassword = Input.CurrentPassword,
            NewPassword = Input.NewPassword
        };

        using var resp = await client.PostAsJsonAsync("api/auth/change-password", dto, JsonDefaults.Options, ct);

        if (resp.IsSuccessStatusCode)
        {
            SuccessMessage = L["ChangePasswordSuccess"];
            ModelState.Clear();
            Input = new();
        }
        else
        {
            // (Optionnel) Tu peux lire le corps pour log DEV, mais ne l’affiche pas brut à l’UI
            // var err = await resp.Content.ReadAsStringAsync(ct);
            Message = L["ChangePasswordError"];
        }

        return Page();
    }
}
