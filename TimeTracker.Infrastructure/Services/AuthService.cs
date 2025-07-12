using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Resources;
using TimeTracker.Core.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTracker.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly IStringLocalizer<Errors> _localizer;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            ApplicationDbContext db,
            IStringLocalizer<Errors> localizer)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
            _db = db;
            _localizer = localizer;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Le nom d’utilisateur est requis.", nameof(request.Username));

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new UnauthorizedAccessException("Identifiants invalides.");

            var signInResult = await _signInManager
                .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
                throw new UnauthorizedAccessException("Identifiants invalides.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var refreshToken = await GenerateAndStoreRefreshToken(user);

            return new LoginResponseDto
            {
                EmployeeId = user.Id,
                Username = user.UserName!,
                Role = Enum.Parse<UserRole>(roles.First()),
                Token = token,
                // Ajoute la propriété si LoginResponseDto la contient
                // RefreshToken = refreshToken
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentException("Le mot de passe est requis.", nameof(model.Password));

            if (model.Role == null)
                throw new ArgumentException("Le rôle est requis.", nameof(model.Role));

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Town = model.Town,
                Country = model.Country,
                Role = model.Role.Value
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
                return false;

            await _userManager.AddToRoleAsync(user, model.Role.Value.ToString());

            return true;
        }

        public async Task<bool> Send2FACodeAsync(Send2FACodeRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null) return false;

            // Générer un code 2FA
            var code = new Random().Next(100000, 999999).ToString();

            // Stocker le code temporairement (à améliorer: stocker en base avec expiration)
            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            // TODO: Envoyer le code par email/SMS
            Console.WriteLine($"Code 2FA pour {user.Email}: {code}");

            return true;
        }

        public async Task<bool> Verify2FACodeAsync(Verify2FACodeRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null
                || string.IsNullOrEmpty(user.TwoFactorCode)
                || user.TwoFactorCodeExpiry < DateTime.UtcNow
                || user.TwoFactorCode != dto.Code)
            {
                return false;
            }

            // Le code est valide, on peut le consommer
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded;
        }

        public async Task<bool> SendForgotPasswordEmailAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return false; // Pour sécurité, ne révèle pas si l'email existe ou non

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO : Envoyer le token par email (ici on log pour dev)
            Console.WriteLine($"Token de réinitialisation pour {user.Email}: {token}");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return false; // Pour sécurité, ne révèle pas si l'email existe ou non

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            return result.Succeeded;
        }

        // Génère un nouveau refresh token et le stocke en base
        private async Task<string> GenerateAndStoreRefreshToken(ApplicationUser user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Par exemple, 7 jours
                UserId = user.Id
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
            return refreshToken.Token;
        }

        // Invalide un refresh token (optionnel, pour one-time use)
        private async Task InvalidateRefreshToken(string token)
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (rt != null)
            {
                rt.Revoked = true;
                await _db.SaveChangesAsync();
            }
        }

        // Endpoint de rafraîchissement du token
        public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var rt = await _db.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && !t.Revoked);

            if (rt == null || rt.ExpiresAt < DateTime.UtcNow)
                return null;

            rt.Revoked = true;
            await _db.SaveChangesAsync();

            var user = rt.User;
            var roles = await _userManager.GetRolesAsync(user);
            var jwt = GenerateJwtToken(user, roles);
            var newRefreshToken = await GenerateAndStoreRefreshToken(user);

            return new RefreshTokenResponseDto
            {
                Token = jwt,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.TryParse(_config["JwtSettings:ExpiresInMinutes"], out var min) ? min : 60)
            };
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwt = _config.GetSection("JwtSettings");
            var secret = jwt["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresInStr = jwt["ExpiresInMinutes"];
            if (!int.TryParse(expiresInStr, out var expiresIn))
                expiresIn = 60;
            var expires = DateTime.UtcNow.AddMinutes(expiresIn);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim("id", user.Id.ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}