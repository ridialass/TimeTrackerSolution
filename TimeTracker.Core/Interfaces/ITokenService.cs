using TimeTracker.Core.Entities;

namespace TimeTracker.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user);
        string GenerateRefreshToken(ApplicationUser user);
        string GeneratePasswordResetToken(ApplicationUser user);
        string Generate2FACode(ApplicationUser user);
        Task<ApplicationUser?> ValidateRefreshTokenAsync(string refreshToken);
        bool Verify2FACode(ApplicationUser user, string code);
    }
}