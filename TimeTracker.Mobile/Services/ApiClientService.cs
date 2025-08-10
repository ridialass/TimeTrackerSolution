// SECURITE :
// Ne jamais logger ni persister le mot de passe ou les identifiants utilisateur dans ce service.
// Ce client doit toujours utiliser HTTPS pour toute communication réseau, surtout en production.
// Seul le token JWT peut être stocké localement (via AuthService), pas le mot de passe.
// Limiter l’exposition des messages d’erreur serveur côté UI (ne jamais retourner une erreur brute du serveur à l’utilisateur final).

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _http;

        // Options JSON homogènes (camelCase, ignore null)
        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public ApiClientService(HttpClient http) => _http = http;

        #region Auth
        public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
        {
            // SECURITE : Le mot de passe n'est jamais loggué, ni persisté.
            try
            {
                var dto = new LoginRequestDto { Username = username, Password = password };
                var res = await _http.PostAsJsonAsync("api/auth/login", dto).ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                {
                    // Journalisation dev uniquement
                    var err = await SafeReadAsync(res).ConfigureAwait(false);
                    Console.WriteLine($"[Auth/Login] HTTP {(int)res.StatusCode} - {err}");
                    return Result<LoginResponseDto>.Fail("Erreur lors de la connexion. Vérifiez vos identifiants ou réessayez plus tard.");
                }

                var login = await res.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpt).ConfigureAwait(false);
                if (login == null)
                    return Result<LoginResponseDto>.Fail("La réponse du serveur est vide ou invalide.");

                return Result<LoginResponseDto>.Success(login);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auth/Login] Exception: {ex.Message}");
                return Result<LoginResponseDto>.Fail("Erreur réseau ou inattendue lors de la connexion.");
            }
        }

        public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/auth/register", dto).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await SafeReadAsync(res).ConfigureAwait(false);
                    Console.WriteLine($"[Auth/Register] HTTP {(int)res.StatusCode} - {err}");
                    return Result<bool>.Fail("Échec de l'inscription. Veuillez vérifier les champs et réessayer.");
                }
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auth/Register] Exception: {ex.Message}");
                return Result<bool>.Fail("Erreur réseau ou inattendue lors de l'inscription.");
            }
        }
        #endregion

        #region Users
        public async Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync()
        {
            try
            {
                var appUsers = await _http.GetFromJsonAsync<IEnumerable<EmployeeDto>>("api/appUsers", JsonOpt).ConfigureAwait(false);
                return Result<IEnumerable<EmployeeDto>>.Success(appUsers ?? new List<EmployeeDto>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Users/Get] Exception: {ex.Message}");
                return Result<IEnumerable<EmployeeDto>>.Fail("Erreur lors du chargement des employés.");
            }
        }
        #endregion

        #region TimeEntries (liste + création + édition)
        public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId)
        {
            try
            {
                var response = await _http.GetAsync($"api/timeentries?userId={userId}").ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await SafeReadAsync(response).ConfigureAwait(false);
                    Console.WriteLine($"[TimeEntries/List] HTTP {(int)response.StatusCode} - {err}");
                    return Result<IEnumerable<TimeEntryDto>>.Fail("Erreur lors du chargement des pointages.");
                }

                var entries = await response.Content.ReadFromJsonAsync<IEnumerable<TimeEntryDto>>(JsonOpt).ConfigureAwait(false);
                return Result<IEnumerable<TimeEntryDto>>.Success(entries ?? new List<TimeEntryDto>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TimeEntries/List] Exception: {ex.Message}");
                return Result<IEnumerable<TimeEntryDto>>.Fail("Erreur lors du chargement des pointages.");
            }
        }

        /// <summary>
        /// Crée un pointage. Si l'API renvoie l'objet créé, on propage l'Id (et champs utiles) dans <paramref name="entry"/>.
        /// </summary>
        public async Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/timeentries", entry, JsonOpt).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await SafeReadAsync(res).ConfigureAwait(false);
                    Console.WriteLine($"[TimeEntries/Create] HTTP {(int)res.StatusCode} - {err}");
                    return Result<bool>.Fail("Échec de l'enregistrement du pointage.");
                }

                // Essaye de récupérer l'objet créé pour propager l'Id côté client
                var created = await res.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpt).ConfigureAwait(false);
                if (created != null)
                {
                    entry.Id = created.Id;
                    entry.UserId = created.UserId;
                    entry.Username = created.Username ?? entry.Username;
                    // Si l’API renvoie des Pauses ou d’autres champs calculés, on peut les recopier ici.
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TimeEntries/Create] Exception: {ex.Message}");
                return Result<bool>.Fail("Erreur lors de l'enregistrement du pointage.");
            }
        }

        /// <summary>Charge un pointage complet (pauses incluses) pour l’édition.</summary>
        public async Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(int id)
        {
            try
            {
                var res = await _http.GetAsync($"api/timeentries/{id}").ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await SafeReadAsync(res).ConfigureAwait(false);
                    Console.WriteLine($"[TimeEntries/GetById] HTTP {(int)res.StatusCode} - {err}");
                    return Result<TimeEntryDto>.Fail("Impossible de charger le pointage demandé.");
                }

                var dto = await res.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpt).ConfigureAwait(false);
                if (dto == null)
                    return Result<TimeEntryDto>.Fail("La réponse du serveur est vide ou invalide.");

                return Result<TimeEntryDto>.Success(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TimeEntries/GetById] Exception: {ex.Message}");
                return Result<TimeEntryDto>.Fail("Erreur lors du chargement du pointage.");
            }
        }

        /// <summary>Met à jour un pointage (inclut la resynchronisation des pauses côté API).</summary>
        public async Task<Result<bool>> UpdateTimeEntryAsync(TimeEntryDto entry)
        {
            try
            {
                if (entry.Id <= 0)
                    return Result<bool>.Fail("Identifiant de pointage invalide.");

                var res = await _http.PutAsJsonAsync($"api/timeentries/{entry.Id}", entry, JsonOpt).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var err = await SafeReadAsync(res).ConfigureAwait(false);
                    Console.WriteLine($"[TimeEntries/Update] HTTP {(int)res.StatusCode} - {err}");
                    return Result<bool>.Fail("Échec de la mise à jour du pointage.");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TimeEntries/Update] Exception: {ex.Message}");
                return Result<bool>.Fail("Erreur lors de la mise à jour du pointage.");
            }
        }
        #endregion

        #region Helpers
        private static async Task<string> SafeReadAsync(HttpResponseMessage res)
        {
            try { return await res.Content.ReadAsStringAsync().ConfigureAwait(false); }
            catch { return "<no-content>"; }
        }
        #endregion
    }
}
