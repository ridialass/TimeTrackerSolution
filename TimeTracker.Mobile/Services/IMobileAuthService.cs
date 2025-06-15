using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public interface IMobileAuthService
    {
        /// <summary>
        /// L’utilisateur courant, ou null s’il n’est pas encore restauré/enregistré.
        /// </summary>
        ApplicationUser? CurrentUser { get; }

        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> RegisterAsync(string username, string password, UserRole role);
        Task<string?> GetTokenAsync();

        /// <summary>
        /// Tente de recharger la session (JWT stocké) et remplit CurrentUser.
        /// </summary>
        Task<bool> TryRestoreSessionAsync();
    }

}
