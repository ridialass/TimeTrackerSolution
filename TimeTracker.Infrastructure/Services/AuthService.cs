using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request)
        {
            // 0) Valider le paramètre
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Le nom d’utilisateur est requis.", nameof(request.Username));

            // 1) retrouver l’utilisateur
            var user = await _userManager.FindByNameAsync(userName: request.Username);
            if (user == null)
                throw new UnauthorizedAccessException("Identifiants invalides.");

            // 2) vérifier le mot de passe
            var signInResult = await _signInManager
                .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
                throw new UnauthorizedAccessException("Identifiants invalides.");

            // 3) récupérer les rôles
            var roles = await _userManager.GetRolesAsync(user);

            // 4) générer le JWT
            var token = GenerateJwtToken(user, roles);

            return new LoginResponseDto
            {
                EmployeeId = user.Id,
                Username = user.UserName!,
                Role = Enum.Parse<UserRole>(roles.First()),
                Token = token
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto model)
        {
            // 0) Validation simple
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new ArgumentException("Le mot de passe est requis.", nameof(model.Password));

            // 1) on peut valider model ici si besoin…

            // 2) créer l’ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Town = model.Town,
                Country = model.Country,
                Role = model.Role
            };

            // 3) créer avec mot de passe
            string userPassword = model.Password;
            var createResult = await _userManager.CreateAsync(user, userPassword);
            if (!createResult.Succeeded)
                return false;

            // 4) assigner le rôle
            await _userManager.AddToRoleAsync(user, model.Role.ToString());

            return true;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequestDto dto, string username)
        {
            // 0) Valider les paramètres
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Le nom d'utilisateur est requis.", nameof(username));
            
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new ArgumentException("Le mot de passe actuel est requis.", nameof(dto.CurrentPassword));
            
            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new ArgumentException("Le nouveau mot de passe est requis.", nameof(dto.NewPassword));

            // 1) Retrouver l'utilisateur
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new UnauthorizedAccessException("Utilisateur non trouvé.");

            // 2) Vérifier le mot de passe actuel
            var signInResult = await _signInManager
                .CheckPasswordSignInAsync(user, dto.CurrentPassword, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
                throw new UnauthorizedAccessException("Mot de passe actuel invalide.");

            // 3) Changer le mot de passe
            var changeResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!changeResult.Succeeded)
            {
                var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Échec du changement de mot de passe: {errors}");
            }
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwt = _config.GetSection("JwtSettings");
            var secret = jwt["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Charge ExpiresInMinutes en int, default à 60 si absent
            var expiresInStr = jwt["ExpiresInMinutes"];
            if (!int.TryParse(expiresInStr, out var expiresIn))
                expiresIn = 60;    // valeur par défaut
            var expires = DateTime.UtcNow.AddMinutes(expiresIn);

            // claims de base + rôle(s)
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
