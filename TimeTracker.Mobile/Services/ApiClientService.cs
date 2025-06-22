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
        try
        {
            var dto = new LoginRequestDto { Username = username, Password = password };
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);
            if (!res.IsSuccessStatusCode)
                return Result<LoginResponseDto>.Fail(await res.Content.ReadAsStringAsync());

            var login = await res.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (login == null)
                return Result<LoginResponseDto>.Fail("La réponse du serveur est vide ou invalide.");

            return Result<LoginResponseDto>.Success(login);
        }
        catch (System.Exception ex)
        {
            return Result<LoginResponseDto>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", dto);
            if (!res.IsSuccessStatusCode)
                return Result<bool>.Fail(await res.Content.ReadAsStringAsync());
            return Result<bool>.Success(true);
        }
        catch (System.Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync()
    {
        try
        {
            var appUsers = await _http.GetFromJsonAsync<IEnumerable<EmployeeDto>>("api/appUsers");
            return Result<IEnumerable<EmployeeDto>>.Success(appUsers ?? new List<EmployeeDto>());
        }
        catch (System.Exception ex)
        {
            return Result<IEnumerable<EmployeeDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId)
    {
        try
        {
            var entries = await _http.GetFromJsonAsync<IEnumerable<TimeEntryDto>>($"api/timeentries?userId={userId}");
            return Result<IEnumerable<TimeEntryDto>>.Success(entries ?? new List<TimeEntryDto>());
        }
        catch (System.Exception ex)
        {
            return Result<IEnumerable<TimeEntryDto>>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/timeentries", entry);
            if (!res.IsSuccessStatusCode)
                return Result<bool>.Fail(await res.Content.ReadAsStringAsync());
            return Result<bool>.Success(true);
        }
        catch (System.Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }
}