// TimeTracker.AdminUI/Pages/Account/ResetPasswordByToken.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TimeTracker.AdminUI.Serialization;   // JsonDefaults.Options
using TimeTracker.Core.Resources;          // Errors.resx

namespace TimeTracker.AdminUI.Pages.Account;

public class ResetPasswordByTokenModel(IHttpClientFactory http, IStringLocalizer<Errors>
    L) : PageModel
{
    [BindProperty, Required]
    public string Token { get; set; } = string.Empty;

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required, DataType(DataType.Password), MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty, Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Message { get; set; }

    // GET /Account/ResetPasswordByToken?token=...&email=...
    public IActionResult OnGet(string? token, string? email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            Message = L["ResetPassword_InvalidLink"]; // ajoute la clé dans Errors.resx
            return Page();
        }

        Token = WebUtility.UrlDecode(token);
        Email = email.Trim();
        return Page();
    }

    public async Task<IActionResult>
        OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var client = http.CreateClient("TimeTrackerAPI");

        // Adapte l’endpoint si ton backend utilise un autre chemin (ex: "api/auth/reset-password-by-token")
        var payload = new { email = Email.Trim(), token = Token, newPassword = NewPassword };

        using var resp = await client.PostAsJsonAsync("api/auth/reset-password", payload, JsonDefaults.Options, ct);

        if (resp.IsSuccessStatusCode)
        {
            Message = L["ResetPassword_Success"]; // ex: "Votre mot de passe a été réinitialisé. Vous pouvez vous connecter."
            ModelState.Clear();
            Token = Email = NewPassword = ConfirmPassword = string.Empty;
        }
        else
        {
            // (Optionnel) logger le détail serveur côté dev : var err = await resp.Content.ReadAsStringAsync(ct);
            Message = L["ResetPassword_Error"]; // ex: "Lien invalide ou expiré."
        }

        return Page();
    }
}
