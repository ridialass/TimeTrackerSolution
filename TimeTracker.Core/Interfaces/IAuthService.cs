using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request);
        Task<bool> RegisterAsync(RegisterRequestDto model);
    }
}
