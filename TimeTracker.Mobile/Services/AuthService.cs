// SECURITE :
// Ne jamais logger ni persister le mot de passe utilisateur dans ce service.
// Toujours transmettre les identifiants via HTTPS et uniquement via POST (jamais URL).
// Seul le token JWT peut être stocké localement, pas le mot de passe.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Mobile.Utils;
using TimeTracker.Mobile.Models;

namespace TimeTracker.Mobile.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "jwt_token";
    private readonly IApiClientService _apiClient;
    private readonly ISecureStorageService _secureStorage;

    public ApplicationUserSession? CurrentUser { get; private set; }

    public AuthService(IApiClientService apiClient, ISecureStorageService secureStorage)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
    {
        // Le mot de passe n'est jamais loggué ni stocké ici. 
        var result = await _apiClient.LoginAsync(username, password);
        if (result.IsSuccess && !string.IsNullOrEmpty(result.Value?.Token))
        {
            await _secureStorage.SetAsync(TokenKey, result.Value.Token);
            CurrentUser = ParseJwt(result.Value.Token);
        }
        return result;
    }

    public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
    {
        // L'API doit imposer que seuls les admins peuvent enregistrer des utilisateurs.
        return await _apiClient.RegisterAsync(dto);
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        _secureStorage.Remove(TokenKey);
        await Task.CompletedTask;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await _secureStorage.GetAsync(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return false;
        CurrentUser = ParseJwt(token);
        // Supprimer immédiatement le token expiré
        if (CurrentUser == null && !string.IsNullOrWhiteSpace(token))
            await _secureStorage.RemoveAsync(TokenKey);
        return CurrentUser != null;
    }

    private ApplicationUserSession? ParseJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var username = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "unique_name" || c.Type == ClaimTypes.Name)?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Si le token est expiré, retourne null (et il sera supprimé)
            if (jwt.ValidTo < DateTime.UtcNow) return null;

            if (!int.TryParse(idClaim, out var userId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                return null;

            return new ApplicationUserSession
            {
                Id = userId,
                UserName = username,
                Role = role,
                JwtToken = token
            };
        }
        catch
        {
            return null;
        }
    }
}