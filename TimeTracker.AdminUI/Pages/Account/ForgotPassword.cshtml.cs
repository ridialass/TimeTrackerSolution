// TimeTracker.AdminUI/Pages/Account/ForgotPassword.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TimeTracker.AdminUI.Serialization;   // JsonDefaults.Options
using TimeTracker.Core.Resources;          // Localisation (Errors)

namespace TimeTracker.AdminUI.Pages.Account;

public class ForgotPasswordModel(IHttpClientFactory http, IStringLocalizer<Errors> L) : PageModel
{
    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Message { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var client = http.CreateClient("TimeTrackerAPI");

        // On reste minimal côté charge utile et on n’expose jamais l’erreur brute serveur à l’UI.
        var payload = new { email = Email.Trim() };

        using var resp = await client.PostAsJsonAsync("api/auth/forgot-password", payload, JsonDefaults.Options, ct);

        Message = resp.IsSuccessStatusCode
            ? L["ForgotPasswordSuccess"]
            : L["ForgotPasswordError"];

        return Page();
    }
}
