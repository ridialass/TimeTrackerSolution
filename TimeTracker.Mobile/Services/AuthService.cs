// SECURITE :
// - Ne jamais logger ni persister le mot de passe utilisateur ici.
// - Transmettre les identifiants via HTTPS et uniquement via POST.
// - Seul le token JWT est stocké localement (SecureStorage). Jamais le mot de passe.

using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Models;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services
{
    public class AuthService : IAuthService
    {
        // Clés de stockage (alignées avec le handler HTTP qui ajoute "Authorization: Bearer ...")
        private const string AccessTokenKey = TokenStorageKeys.AccessToken; // "auth_access_token"
        private const string LegacyTokenKey = "jwt_token";                  // compat ancien code
        private const string SessionKey = "auth_user_session";

        private readonly IApiClientService _apiClient;
        private readonly ISecureStorageService _secureStorage;
        private readonly ILogger<AuthService> _logger;

        public ApplicationUserSession? CurrentUser { get; private set; }

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public AuthService(IApiClientService apiClient, ISecureStorageService secureStorage, ILogger<AuthService> logger)
        {
            _apiClient = apiClient;
            _secureStorage = secureStorage;
            _logger = logger;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
        {
            var result = await _apiClient.LoginAsync(username, password);
            if (!result.IsSuccess || result.Value is null)
            {
                _logger.LogWarning("[AuthService/Login] API login failed: {Error}", result.Error);
                return result;
            }

            var login = result.Value;

            // Ton DTO expose Token + ApplicationUserId + Username + Role (enum)
            var token = login.Token;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("[AuthService/Login] Missing token in server response.");
                return Result<LoginResponseDto>.Fail("Le jeton d'accès est manquant dans la réponse du serveur.");
            }

            // Stocker le token (nouvelle clé + compat)
            await _secureStorage.SetAsync(AccessTokenKey, token);
            await _secureStorage.SetAsync(LegacyTokenKey, token);

            // Construire la session à partir du JWT (et compléter avec la réponse du serveur)
            var session = ParseJwtToSession(token) ?? new ApplicationUserSession();

            // Compléments/Overlays depuis la réponse serveur
            if (login.ApplicationUserId > 0) session.Id = login.ApplicationUserId;
            if (!string.IsNullOrWhiteSpace(login.Username)) session.UserName = login.Username;
            // Role est un enum côté DTO → on stocke en string dans la session si manquant
            if (string.IsNullOrWhiteSpace(session.Role)) session.Role = login.Role.ToString();

            // Valeurs de repli
            session.UserName ??= username;
            session.JwtToken = token;

            CurrentUser = session;
            // 🔎 LOG ICI — juste après avoir bâti la session
            _logger.LogInformation("[AuthService/Login] Session built: tokenNull={TokenNull}, id={Id}, role={Role}, user={User}",
                string.IsNullOrWhiteSpace(session.JwtToken),
                session.Id,
                session.Role,
                session.UserName);

            // Persister la session sérialisée pour restauration rapide
            var json = JsonSerializer.Serialize(session, JsonOpt);
            await _secureStorage.SetAsync(SessionKey, json);


            return result;
        }

        public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
        {
            // Déléguée à l’API (qui devrait imposer que seuls les admins peuvent créer des comptes)
            return await _apiClient.RegisterAsync(dto);
        }

        public async Task LogoutAsync()
        {
            CurrentUser = null;
            await _secureStorage.RemoveAsync(AccessTokenKey);
            await _secureStorage.RemoveAsync(LegacyTokenKey);
            await _secureStorage.RemoveAsync(SessionKey);
        }

        public async Task<bool> TryRestoreSessionAsync()
        {
            // 1) Récupérer token (nouvelle clé), sinon migrer depuis l’ancienne
            var token = await _secureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                var legacy = await _secureStorage.GetAsync(LegacyTokenKey);
                if (!string.IsNullOrWhiteSpace(legacy))
                {
                    token = legacy;
                    // Migration douce vers la nouvelle clé
                    await _secureStorage.SetAsync(AccessTokenKey, legacy);
                }
            }

            if (string.IsNullOrWhiteSpace(token))
                return false;

            // 2) Tenter de recharger la session sérialisée
            var json = await _secureStorage.GetAsync(SessionKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var cached = JsonSerializer.Deserialize<ApplicationUserSession>(json!, JsonOpt);
                    if (cached != null && !IsJwtExpired(token))
                    {
                        cached.JwtToken = token; // cohérence
                        // Renseigner l'expiration si elle n'était pas stockée
                        var exp = GetJwtExpiryUtc(token);
                        if (exp.HasValue) cached.ExpiresAtUtc = exp.Value;
                        CurrentUser = cached;
                        _logger.LogInformation("[AuthService/TryRestore] Restored from cache: id={Id}, role={Role}", cached.Id, cached.Role);
                        return true;
                    }
                }
                catch
                {
                    // Session corrompue → on reconstruit depuis le JWT
                }
            }

            // 3) Reconstruction depuis le JWT
            if (IsJwtExpired(token))
            {
                await _secureStorage.RemoveAsync(AccessTokenKey);
                await _secureStorage.RemoveAsync(LegacyTokenKey);
                await _secureStorage.RemoveAsync(SessionKey);
                CurrentUser = null;
                return false;
            }

            var session = ParseJwtToSession(token);
            if (session == null)
            {
                CurrentUser = null;
                return false;
            }

            session.JwtToken = token;
            CurrentUser = session;

            var rebuilt = JsonSerializer.Serialize(session, JsonOpt);
            await _secureStorage.SetAsync(SessionKey, rebuilt);

            return true;
        }

        // -------- Helpers --------

        private static bool IsJwtExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.ValidTo <= DateTime.UtcNow; // ValidTo est UTC
            }
            catch
            {
                return true;
            }
        }

        private static DateTimeOffset? GetJwtExpiryUtc(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero);
            }
            catch
            {
                return null;
            }
        }

        private static ApplicationUserSession? ParseJwtToSession(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo <= DateTime.UtcNow) return null;

                // Id (essaie plusieurs claims courants)
                var idClaim = jwt.Claims.FirstOrDefault(c =>
                                   c.Type == "nameid" ||
                                   c.Type == "userId" ||
                                   c.Type == "uid" ||
                                   c.Type == "id" ||
                                   c.Type == ClaimTypes.NameIdentifier ||
                                   c.Type == JwtRegisteredClaimNames.Sub);
                int.TryParse(idClaim?.Value, out var userId);

                // Username
                var userName = jwt.Claims.FirstOrDefault(c =>
                                   c.Type == "unique_name" ||
                                   c.Type == "username" ||
                                   c.Type == ClaimTypes.Name ||
                                   c.Type == JwtRegisteredClaimNames.UniqueName ||
                                   c.Type == "name" ||
                                   c.Type == JwtRegisteredClaimNames.Sub)
                                   ?.Value;

                // Role (string ou array → on concatène si besoin)
                string? role = null;
                var roleClaims = jwt.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                    .Select(c => c.Value)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToArray();

                if (roleClaims.Length == 1) role = roleClaims[0];
                else if (roleClaims.Length > 1) role = string.Join(",", roleClaims);

                if (userId <= 0 && string.IsNullOrWhiteSpace(userName))
                    return null;

                return new ApplicationUserSession
                {
                    Id = userId,
                    UserName = userName ?? string.Empty,
                    Role = role ?? string.Empty,
                    ExpiresAtUtc = new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero)
                };
            }
            catch
            {
                return null;
            }
        }

        public static class TokenStorageKeys
        {
            public const string AccessToken = "auth_access_token";
            public const string RefreshToken = "auth_refresh_token"; // si tu l’utilises
        }
    }
}
