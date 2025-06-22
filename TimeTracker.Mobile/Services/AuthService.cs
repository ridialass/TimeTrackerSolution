using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
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
        // Let the API enforce that only admins can register users
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