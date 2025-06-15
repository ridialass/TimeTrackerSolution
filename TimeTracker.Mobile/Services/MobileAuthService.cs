// MobileAuthService.cs
using Microsoft.Maui.Storage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public class MobileAuthService : IMobileAuthService
    {
        private readonly IApiClientService _apiClient;
        private readonly ISecureStorage _secureStorage;
        private const string TOKEN_KEY = "jwt_token";

        public ApplicationUser? CurrentUser { get; private set; }

        public MobileAuthService(
            IApiClientService apiClient,
            ISecureStorage secureStorage)
        {
            _apiClient = apiClient;
            _secureStorage = secureStorage;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var dto = new LoginRequestDto { Username = username, Password = password };
            var json = JsonSerializer.Serialize(dto);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Appelle votre API /api/auth/login
            var response = await _apiClient.PostAsync("api/auth/login", content);
            if (!response.IsSuccessStatusCode)
                return false;

            var resp = await response.Content.ReadAsStringAsync();
            var login = JsonSerializer.Deserialize<LoginResponseDto>(
                resp,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (login == null || string.IsNullOrWhiteSpace(login.Token))
                return false;

            // 1) Stocke le token
            await _secureStorage.SetAsync(TOKEN_KEY, login.Token);

            // (Le AuthHeaderHandler va le prendre automatiquement dans SecureStorage)

            // 2) Met à jour CurrentUser
            CurrentUser = new ApplicationUser
            {
                Id = login.EmployeeId,
                UserName = login.Username,
                Role = login.Role
            };

            return true;
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _secureStorage.GetAsync(TOKEN_KEY);
            }
            catch
            {
                return null;
            }
        }

        public Task LogoutAsync()
        {
            CurrentUser = null;
            _secureStorage.Remove(TOKEN_KEY);
            return Task.CompletedTask;
        }

        public async Task<bool> RegisterAsync(string username, string password, UserRole role)
        {
            var dto = new RegisterRequestDto
            {
                Username = username,
                Password = password,
                Role = role,
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Town = string.Empty,
                Country = string.Empty
            };
            var json = JsonSerializer.Serialize(dto);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _apiClient.PostAsync("api/auth/register", content);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> TryRestoreSessionAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            // 3) Décode le JWT en local pour en extraire les claims
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt;
            try
            {
                jwt = handler.ReadJwtToken(token);
            }
            catch
            {
                return false;
            }

            // 4) Lit les claims "UserId", "name" et "role"
            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (!int.TryParse(idClaim, out var userId)
             || string.IsNullOrEmpty(nameClaim)
             || string.IsNullOrEmpty(roleClaim))
            {
                return false;
            }

            // 5) On remplit CurrentUser
            CurrentUser = new ApplicationUser
            {
                Id = userId,
                UserName = nameClaim,
                Role = Enum.Parse<UserRole>(roleClaim, ignoreCase: true)
            };

            return true;
        }

    }
}
