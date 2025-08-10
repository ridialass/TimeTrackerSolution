using System;
using System.Linq;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Models
{
    /// <summary>
    /// Lightweight user session for mobile authentication.
    /// - Ne stocke jamais le mot de passe.
    /// - Le JWT est également persisté de façon sécurisée via ISecureStorageService.
    /// </summary>
    public class ApplicationUserSession
    {
        /// <summary>Utilisateur (issu du JWT ou de la réponse login).</summary>
        public int Id { get; set; }

        /// <summary>Nom d’utilisateur (issu du JWT ou de la réponse login).</summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>Rôle au format texte (ex: "Admin", "Technician", ou "Admin,Manager").</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>JWT courant (en mémoire). La copie persistée est gérée par ISecureStorageService.</summary>
        public string JwtToken { get; set; } = string.Empty;

        /// <summary>Instant d’expiration du JWT en UTC si connu (peut être renseigné lors du parsing).</summary>
        public DateTimeOffset? ExpiresAtUtc { get; set; }

        /// <summary>Session valide si Id>0, token présent et non expiré (si expiration connue).</summary>
        public bool IsAuthenticated =>
            Id > 0 &&
            !string.IsNullOrWhiteSpace(JwtToken) &&
            (ExpiresAtUtc is null || ExpiresAtUtc > DateTimeOffset.UtcNow);

        /// <summary>
        /// Vue pratique du rôle en enum si la valeur texte correspond à UserRole (ignore la casse).
        /// Si plusieurs rôles sont présents (séparés par virgules), renvoie le premier match.
        /// </summary>
        public UserRole? RoleEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Role)) return null;

                foreach (var part in Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (Enum.TryParse<UserRole>(part, true, out var parsed))
                        return parsed;
                }
                return null;
            }
        }

        /// <summary>True si le rôle contient "Admin" (ou RoleEnum==Admin).</summary>
        public bool IsAdmin
        {
            get
            {
                if (RoleEnum is UserRole.Admin) return true;
                if (string.IsNullOrWhiteSpace(Role)) return false;

                return Role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            }
        }

        public override string ToString() => $"{UserName} ({Role})";
    }
}
