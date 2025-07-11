using System.Threading.Tasks;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request);
        Task<bool> RegisterAsync(RegisterRequestDto model);

        // Ajout pour mot de passe oublié
        Task<bool> SendForgotPasswordEmailAsync(ForgotPasswordRequestDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto);
        // Ajout pour refresh token
        Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto, string username);
        Task<bool> Send2FACodeAsync(Send2FACodeRequestDto dto);
        Task<bool> Verify2FACodeAsync(Verify2FACodeRequestDto dto);
    }
}