using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RazorLight;
using System;
using System.IO;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly RazorLightEngine _razorEngine;

        public EmailService(IConfiguration config)
        {
            _config = config;
            // Chemin absolu vers le dossier EmailTemplates à la racine de TimeTracker.Infrastructure
            var templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesPath)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Réinitialisation de votre mot de passe";
            var model = new PasswordResetEmailModel { ResetLink = resetLink };
            var body = await _razorEngine.CompileRenderAsync("PasswordResetEmail.cshtml", model);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config["Smtp:SenderName"], _config["Smtp:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            int port = int.TryParse(_config["Smtp:Port"], out var parsedPort) ? parsedPort : 25;
            bool useSsl = bool.TryParse(_config["Smtp:UseSsl"], out var parsedUseSsl) && parsedUseSsl;

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_config["Smtp:Host"], port, useSsl);
            await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task Send2FACodeAsync(string email, string code)
        {
            var subject = "Votre code d'authentification";
            var model = new Send2FAEmailModel { Code = code };
            var body = await _razorEngine.CompileRenderAsync("Send2FA.cshtml", model);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config["Smtp:SenderName"], _config["Smtp:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            int port = int.TryParse(_config["Smtp:Port"], out var parsedPort) ? parsedPort : 25; // 25 = port SMTP par défaut
            var useSsl = bool.TryParse(_config["Smtp:UseSsl"], out var parsedUseSsl) && parsedUseSsl;
            Console.WriteLine("[EMAIL] Tentative d'envoi à " + email);
            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                

                var socketOptions = useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await client.ConnectAsync(_config["Smtp:Host"], port, useSsl);
                await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine($"Email envoyé à {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email à {email} : {ex.Message}");
                throw;
            }
        }
    }

}