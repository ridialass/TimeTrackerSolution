using System.Threading.Tasks;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            // Ici tu mettrais la logique d'envoi d'email réel (SMTP, SendGrid, etc.)
            // Exemple fictif en console :
            await Task.Run(() =>
            {
                // Simule l'envoi d'un email
                System.Console.WriteLine($"[Email] Password reset sent to {email} with token: {token}");
            });
        }

        public async Task Send2FACodeAsync(string email, string code)
        {
            // Ici tu mettrais la logique d'envoi d'email réel (SMTP, SendGrid, etc.)
            // Exemple fictif en console :
            await Task.Run(() =>
            {
                // Simule l'envoi d'un email
                System.Console.WriteLine($"[Email] 2FA code sent to {email}: {code}");
            });
        }
    }
}