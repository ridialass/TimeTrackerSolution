using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public interface IMobileAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> RegisterAsync(string username, string password, UserRole role);
        Task<string?> GetTokenAsync();

        // ← on ajoute :
        Task<bool> TryRestoreSessionAsync();
    }

}
