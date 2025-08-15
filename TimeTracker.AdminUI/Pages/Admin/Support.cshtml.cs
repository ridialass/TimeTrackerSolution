// Pages/Admin/Support.cshtml.cs
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace TimeTracker.AdminUI.Pages.Admin;

[Authorize(Roles = "Admin")]
public class SupportModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly IStringLocalizer<SharedResource> _L;

    public SupportModel(IConfiguration config, IStringLocalizer<SharedResource> localizer)
    {
        _config = config;
        _L = localizer;
    }

    // ----- FAQ -----
    public record FaqItem(string Id, string Title, string Body);
    public List<FaqItem> Faq { get; private set; } = new();

    // ----- Formulaire contact -----
    [BindProperty, Required(ErrorMessage = "RequiredField")]
    public string Topic { get; set; } = "general";

    [BindProperty, EmailAddress(ErrorMessage = "InvalidEmail")]
    public string? Email { get; set; }

    [BindProperty, Required(ErrorMessage = "RequiredField")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "MessageLength")]
    public string Message { get; set; } = "";

    [BindProperty]
    public bool IncludeTechnical { get; set; } = true;

    public string? Success { get; set; }
    public string? Error { get; set; }
    public string SupportTo => _config["Support:Email"] ?? "support@timetracker.it";

    public void OnGet()
    {
        BuildFaq();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        BuildFaq();

        if (!ModelState.IsValid)
        {
            Error = _L["Support_Form_Invalid"];
            return Page();
        }

        // SMTP depuis appsettings:
        // "Smtp": { "Host": "...", "Port": 587, "User": "...", "Password": "...", "From": "no-reply@timetracker.it", "Ssl": true }
        var host = _config["Smtp:Host"];
        var from = _config["Smtp:From"] ?? "no-reply@timetracker.it";
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
        var ssl = bool.TryParse(_config["Smtp:Ssl"], out var s) ? s : true;
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];
        var to = SupportTo;

        var subject = $"[TimeTracker Support] {Topic} — {User?.Identity?.Name ?? "Unknown"}";
        var body = BuildEmailBody();

        if (string.IsNullOrWhiteSpace(host))
        {
            // Repli : SMTP non configuré -> message d’info et lien mailto côté vue
            Error = _L["Support_Smtp_NotConfigured"];
            return Page();
        }

        try
        {
            using var msg = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            if (!string.IsNullOrWhiteSpace(Email))
            {
                msg.ReplyToList.Add(new MailAddress(Email));
            }

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(user, pass)
            };

            await smtp.SendMailAsync(msg);
            Success = _L["Support_Sent_Success"];
            // Reset form
            Message = "";
            Topic = "general";
            IncludeTechnical = true;
        }
        catch (Exception)
        {
            Error = _L["Support_Sent_Error"];
        }

        return Page();
    }

    private string BuildEmailBody()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Topic: {Topic}");
        sb.AppendLine($"From user: {User?.Identity?.Name}");
        var roleClaims = User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
        sb.AppendLine($"Roles: {string.Join(", ", roleClaims)}");
        if (!string.IsNullOrWhiteSpace(Email))
            sb.AppendLine($"Reply-To: {Email}");
        sb.AppendLine();
        sb.AppendLine("Message:");
        sb.AppendLine(Message);
        sb.AppendLine();

        if (IncludeTechnical)
        {
            sb.AppendLine("---- Technical ----");
            sb.AppendLine($"Culture: {System.Globalization.CultureInfo.CurrentUICulture}");
            sb.AppendLine($"Path: {Request?.Path.ToString()}");
            sb.AppendLine($"User-Agent: {Request?.Headers["User-Agent"].ToString()}");
            sb.AppendLine($"Time (UTC): {DateTime.UtcNow:O}");
        }
        return sb.ToString();
    }

    private void BuildFaq()
    {
        // Les textes viennent du SharedResource (une seule resx par langue)
        Faq = new()
        {
            new("q1", _L["Support_FAQ_Login_Title"],        _L["Support_FAQ_Login_Body"]),
            new("q2", _L["Support_FAQ_Reset_Title"],        _L["Support_FAQ_Reset_Body"]),
            new("q3", _L["Support_FAQ_Roles_Title"],        _L["Support_FAQ_Roles_Body"]),
            new("q4", _L["Support_FAQ_Export_Title"],       _L["Support_FAQ_Export_Body"]),
            new("q5", _L["Support_FAQ_Billing_Title"],      _L["Support_FAQ_Billing_Body"]),
            new("q6", _L["Support_FAQ_Bug_Title"],          _L["Support_FAQ_Bug_Body"])
        };
    }
}
