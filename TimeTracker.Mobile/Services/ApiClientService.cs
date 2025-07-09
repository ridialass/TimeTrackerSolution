// SECURITE :
// Ne jamais logger ni persister le mot de passe ou les identifiants utilisateur dans ce service.
// Ce client doit toujours utiliser HTTPS pour toute communication réseau, surtout en production.
// Seul le token JWT peut être stocké localement (via AuthService), pas le mot de passe.
// Limiter l’exposition des messages d’erreur serveur côté UI (ne jamais retourner une erreur brute du serveur à l’utilisateur final).

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _http;

    public ApiClientService(HttpClient http) => _http = http;

    public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
    {
        // SECURITE : Le mot de passe n'est jamais loggué, ni persisté.
        try
        {
            var dto = new LoginRequestDto { Username = username, Password = password };
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);

            if (!res.IsSuccessStatusCode)
                // Ne retourne pas l'erreur brute du serveur à l'utilisateur.
                return Result<LoginResponseDto>.Fail("Erreur lors de la connexion. Veuillez vérifier vos identifiants ou réessayer plus tard.");

            var login = await res.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (login == null)
                return Result<LoginResponseDto>.Fail("La réponse du serveur est vide ou invalide.");

            return Result<LoginResponseDto>.Success(login);
        }
        catch (System.Exception)
        {
            // Ne jamais inclure le mot de passe ni de détails sensibles dans la remontée d’erreur
            return Result<LoginResponseDto>.Fail("Erreur réseau ou inattendue lors de la connexion.");
        }
    }

    public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", dto);
            if (!res.IsSuccessStatusCode)
                return Result<bool>.Fail("Échec de l'inscription. Veuillez vérifier les champs et réessayer.");
            return Result<bool>.Success(true);
        }
        catch (System.Exception)
        {
            return Result<bool>.Fail("Erreur réseau ou inattendue lors de l'inscription.");
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync()
    {
        try
        {
            var appUsers = await _http.GetFromJsonAsync<IEnumerable<EmployeeDto>>("api/appUsers");
            return Result<IEnumerable<EmployeeDto>>.Success(appUsers ?? new List<EmployeeDto>());
        }
        catch (System.Exception)
        {
            return Result<IEnumerable<EmployeeDto>>.Fail("Erreur lors du chargement des employés.");
        }
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId)
    {
        try
        {
            var entries = await _http.GetFromJsonAsync<IEnumerable<TimeEntryDto>>($"api/timeentries?userId={userId}");
            return Result<IEnumerable<TimeEntryDto>>.Success(entries ?? new List<TimeEntryDto>());
        }
        catch (System.Exception)
        {
            return Result<IEnumerable<TimeEntryDto>>.Fail("Erreur lors du chargement des pointages.");
        }
    }

    public async Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/timeentries", entry);
            if (!res.IsSuccessStatusCode)
                return Result<bool>.Fail("Échec de l'enregistrement du pointage.");
            return Result<bool>.Success(true);
        }
        catch (System.Exception)
        {
            return Result<bool>.Fail("Erreur lors de l'enregistrement du pointage.");
        }
    }
}