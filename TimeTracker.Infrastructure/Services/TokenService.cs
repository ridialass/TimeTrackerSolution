using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateToken(ApplicationUser user)
        {
            // Génération d'un token JWT fictif (à remplacer par une vraie implémentation JWT)
            var payload = $"{user.Id}:{user.UserName}:{DateTime.UtcNow.Ticks}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        }

        public string GenerateRefreshToken(ApplicationUser user)
        {
            // Génération d'un refresh token aléatoire, stockable côté DB
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public string GeneratePasswordResetToken(ApplicationUser user)
        {
            // Génération d'un token pour reset password, à stocker dans la DB
            var payload = $"{user.Id}:{user.Email}:{Guid.NewGuid()}:{DateTime.UtcNow.Ticks}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
        }

        public string Generate2FACode(ApplicationUser user)
        {
            // Génération d'un code 2FA à 6 chiffres
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }

        public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string refreshToken)
        {
            // Cette méthode devrait vérifier le refreshToken dans la DB et retourner l'utilisateur associé.
            // Ici, retourne null en placeholder car dépend du contexte d'accès DB.
            await Task.CompletedTask;
            return null;
        }

        public bool Verify2FACode(ApplicationUser user, string code)
        {
            // Ici tu dois comparer le code avec celui stocké/attendu pour l'utilisateur.
            // Placeholder: toujours faux.
            return false;
        }
    }
}