using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _http;

    public ApiClientService(HttpClient http) => _http = http;

    public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
    {
        try
        {
            var dto = new LoginRequestDto { Username = username, Password = password };
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);

            if (!res.IsSuccessStatusCode)
                return Result<LoginResponseDto>.Fail("Erreur lors de la connexion. Veuillez vérifier vos identifiants ou réessayer plus tard.");

            var login = await res.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (login == null)
                return Result<LoginResponseDto>.Fail("La réponse du serveur est vide ou invalide.");

            return Result<LoginResponseDto>.Success(login);
        }
        catch (System.Exception)
        {
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

    public async Task<Result<TimeEntryDto>> CreateTimeEntryAsync(TimeEntryDto entry)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/timeentries", entry);
            if (!res.IsSuccessStatusCode)
                return Result<TimeEntryDto>.Fail("Échec de l'enregistrement du pointage.");
            var created = await res.Content.ReadFromJsonAsync<TimeEntryDto>();
            if (created == null)
                return Result<TimeEntryDto>.Fail("La réponse du serveur est vide ou invalide.");
            return Result<TimeEntryDto>.Success(created);
        }
        catch (System.Exception)
        {
            return Result<TimeEntryDto>.Fail("Erreur lors de l'enregistrement du pointage.");
        }
    }

    public async Task<Result<TimeEntryDto>> UpdateTimeEntryAsync(TimeEntryDto entry)
    {
        try
        {
            var res = await _http.PutAsJsonAsync($"api/timeentries/{entry.Id}", entry);
            if (!res.IsSuccessStatusCode)
                return Result<TimeEntryDto>.Fail("Échec de la mise à jour du pointage.");
            var updated = await res.Content.ReadFromJsonAsync<TimeEntryDto>();
            if (updated == null)
                return Result<TimeEntryDto>.Fail("La réponse du serveur est vide ou invalide.");
            return Result<TimeEntryDto>.Success(updated);
        }
        catch (System.Exception)
        {
            return Result<TimeEntryDto>.Fail("Erreur lors de la mise à jour du pointage.");
        }
    }
}