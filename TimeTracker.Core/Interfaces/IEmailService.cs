using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string token);
        Task Send2FACodeAsync(string email, string code);
    }
}
