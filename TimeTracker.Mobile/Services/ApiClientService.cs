// SECURITE :
// - Ne jamais logger ni persister le mot de passe utilisateur.
// - Toujours utiliser HTTPS en production.
// - UI : messages génériques ; les détails restent dans les logs.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiClientService> _logger;

        // Lecture: tolérante à la casse, écriture: camelCase (peu importe côté client)
        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Écriture stricte PascalCase pour coller au binder serveur (StartTime, EndTime, etc.)
        private static readonly JsonSerializerOptions JsonOptPascal = new()
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static class Api
        {
            public const string Login = "api/auth/login";
            public const string Register = "api/auth/register";
            public const string AppUsers = "api/appUsers";
            public const string TimeEntries = "api/timeentries";
            public static string TimeEntryById(int id) => $"api/timeentries/{id}";
            public static string TimeEntriesByUser(int userId) => $"api/timeentries?userId={userId}";
        }

        public ApiClientService(HttpClient http, ILogger<ApiClientService> logger)
        {
            _http = http;
            _logger = logger;

            if (_http.DefaultRequestHeaders.Accept.Count == 0)
                _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ------------------- Auth -------------------

        public async Task<Result<LoginResponseDto>> LoginAsync(string username, string password)
        {
            try
            {
                var payload = new LoginRequestDto { Username = username, Password = password };
                using var res = await _http.PostAsJsonAsync(Api.Login, payload, JsonOpt).ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    var login = await res.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpt).ConfigureAwait(false);
                    if (login is null)
                        return Result<LoginResponseDto>.Fail("La réponse du serveur est vide ou invalide.");

                    return Result<LoginResponseDto>.Success(login);
                }

                var uiMsg = await MapErrorAsync(res, "[Auth/Login]").ConfigureAwait(false);
                return Result<LoginResponseDto>.Fail(uiMsg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Auth/Login] Erreur réseau (BaseAddress: {Base})", _http.BaseAddress);
                return Result<LoginResponseDto>.Fail("Impossible de contacter le serveur. Vérifiez la connexion.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[Auth/Login] Timeout.");
                return Result<LoginResponseDto>.Fail("La requête a expiré. Réessayez.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth/Login] Erreur inattendue.");
                return Result<LoginResponseDto>.Fail("Erreur inattendue pendant la connexion.");
            }
        }

        public async Task<Result<bool>> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                using var res = await _http.PostAsJsonAsync(Api.Register, dto, JsonOpt).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                    return Result<bool>.Success(true);

                var uiMsg = await MapErrorAsync(res, "[Auth/Register]").ConfigureAwait(false);
                return Result<bool>.Fail(uiMsg);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Auth/Register] Erreur réseau.");
                return Result<bool>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[Auth/Register] Timeout.");
                return Result<bool>.Fail("La requête a expiré. Réessayez.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth/Register] Erreur inattendue.");
                return Result<bool>.Fail("Erreur inattendue pendant l'inscription.");
            }
        }

        // ------------------- Users -------------------

        public async Task<Result<IEnumerable<EmployeeDto>>> GetEmployeesAsync()
        {
            try
            {
                var items = await _http.GetFromJsonAsync<IEnumerable<EmployeeDto>>(Api.AppUsers, JsonOpt).ConfigureAwait(false);
                return Result<IEnumerable<EmployeeDto>>.Success(items ?? new List<EmployeeDto>());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[Users/Get] Erreur réseau.");
                return Result<IEnumerable<EmployeeDto>>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[Users/Get] Timeout.");
                return Result<IEnumerable<EmployeeDto>>.Fail("La requête a expiré.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Users/Get] Erreur inattendue.");
                return Result<IEnumerable<EmployeeDto>>.Fail("Erreur lors du chargement des employés.");
            }
        }

        // ------------------- TimeEntries -------------------

        public async Task<Result<IEnumerable<TimeEntryDto>>> GetTimeEntriesAsync(int userId)
        {
            try
            {
                using var res = await _http.GetAsync(Api.TimeEntriesByUser(userId)).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var uiMsg = await MapErrorAsync(res, "[TimeEntries/List]").ConfigureAwait(false);
                    return Result<IEnumerable<TimeEntryDto>>.Fail(uiMsg);
                }

                var entries = await res.Content.ReadFromJsonAsync<IEnumerable<TimeEntryDto>>(JsonOpt).ConfigureAwait(false);
                return Result<IEnumerable<TimeEntryDto>>.Success(entries ?? new List<TimeEntryDto>());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TimeEntries/List] Erreur réseau.");
                return Result<IEnumerable<TimeEntryDto>>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[TimeEntries/List] Timeout.");
                return Result<IEnumerable<TimeEntryDto>>.Fail("La requête a expiré.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TimeEntries/List] Erreur inattendue.");
                return Result<IEnumerable<TimeEntryDto>>.Fail("Erreur lors du chargement des pointages.");
            }
        }

        // Harmonized: Only ever send completed sessions (with EndTime set) to API
        public async Task<Result<bool>> CreateTimeEntryAsync(TimeEntryDto entry)
        {
            try
            {
                if (!entry.EndTime.HasValue)
                    return Result<bool>.Fail("Impossible de créer un pointage incomplet : EndTime doit être renseigné.");

                // Ensure username is set before sending
                if (string.IsNullOrWhiteSpace(entry.Username))
                {
                    entry.Username = entry.Username ?? string.Empty;
                }

                var payload = PrepareForApi(entry);
                using var res = await _http.PostAsJsonAsync(Api.TimeEntries, payload, JsonOptPascal).ConfigureAwait(false);

                if (!res.IsSuccessStatusCode)
                {
                    var uiMsg = await MapErrorAsync(res, "[TimeEntries/Create]").ConfigureAwait(false);

                    _logger.LogWarning("[CreateEntry] FAILED\nPayload: {Payload}\nError: {Error}",
                        JsonSerializer.Serialize(payload, JsonOptPascal), uiMsg);

                    return Result<bool>.Fail(uiMsg);
                }

                var created = await res.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpt).ConfigureAwait(false);
                if (created is not null)
                {
                    entry.Id = created.Id;
                    entry.UserId = created.UserId;
                    entry.Username = created.Username ?? entry.Username;
                }

                return Result<bool>.Success(true);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TimeEntries/Create] Erreur réseau.");
                return Result<bool>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[TimeEntries/Create] Timeout.");
                return Result<bool>.Fail("La requête a expiré.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TimeEntries/Create] Erreur inattendue.");
                return Result<bool>.Fail("Erreur lors de l'enregistrement du pointage.");
            }
        }

        public async Task<Result<TimeEntryDto>> GetTimeEntryByIdAsync(int id)
        {
            try
            {
                using var res = await _http.GetAsync(Api.TimeEntryById(id)).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var uiMsg = await MapErrorAsync(res, "[TimeEntries/GetById]").ConfigureAwait(false);
                    return Result<TimeEntryDto>.Fail(uiMsg);
                }

                var dto = await res.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpt).ConfigureAwait(false);
                if (dto is null)
                    return Result<TimeEntryDto>.Fail("La réponse du serveur est vide ou invalide.");

                return Result<TimeEntryDto>.Success(dto);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TimeEntries/GetById] Erreur réseau.");
                return Result<TimeEntryDto>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[TimeEntries/GetById] Timeout.");
                return Result<TimeEntryDto>.Fail("La requête a expiré.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TimeEntries/GetById] Erreur inattendue.");
                return Result<TimeEntryDto>.Fail("Erreur lors du chargement du pointage.");
            }
        }

        public async Task<Result<bool>> UpdateTimeEntryAsync(TimeEntryDto entry)
        {
            try
            {
                if (entry.Id <= 0)
                    return Result<bool>.Fail("Identifiant de pointage invalide.");

                // Ensure username is set before sending
                if (string.IsNullOrWhiteSpace(entry.Username))
                {
                    entry.Username = entry.Username ?? string.Empty;
                }

                var payload = PrepareForApi(entry);
                using var res = await _http.PutAsJsonAsync(Api.TimeEntryById(entry.Id), payload, JsonOptPascal).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    var uiMsg = await MapErrorAsync(res, "[TimeEntries/Update]").ConfigureAwait(false);

                    _logger.LogWarning("[UpdateEntry] FAILED\nPayload: {Payload}\nError: {Error}",
                        JsonSerializer.Serialize(payload, JsonOptPascal), uiMsg);

                    return Result<bool>.Fail(uiMsg);
                }

                return Result<bool>.Success(true);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TimeEntries/Update] Erreur réseau.");
                return Result<bool>.Fail("Impossible de contacter le serveur.");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("[TimeEntries/Update] Timeout.");
                return Result<bool>.Fail("La requête a expiré.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TimeEntries/Update] Erreur inattendue.");
                return Result<bool>.Fail("Erreur lors de la mise à jour du pointage.");
            }
        }

        // ------------------- Helpers -------------------

        /// <summary>
        /// Prépare un DTO pour l’API (noms PascalCase + cohérence des champs).
        /// - Si IncludesTravelTime == false => TravelDurationHours = null.
        /// - Nettoie les pauses sans End.
        /// - Assure EndTime >= StartTime si EndTime fourni (sinon laisse tel quel).
        /// - N’envoie pas TotalPauseDuration (calculée côté serveur).
        /// </summary>
        private static TimeEntryDto PrepareForApi(TimeEntryDto src)
        {
            var clone = new TimeEntryDto
            {
                Id = src.Id,
                StartTime = src.StartTime,           // JSON prend en charge les secondes/millis
                EndTime = src.EndTime,
                StartLatitude = src.StartLatitude,
                StartLongitude = src.StartLongitude,
                StartAddress = src.StartAddress,
                EndLatitude = src.EndLatitude,
                EndLongitude = src.EndLongitude,
                EndAddress = src.EndAddress,
                SessionType = src.SessionType,
                IsAdminModified = src.IsAdminModified,
                IncludesTravelTime = src.IncludesTravelTime,
                TravelDurationHours = src.IncludesTravelTime ? src.TravelDurationHours : null,
                DinnerPaid = src.DinnerPaid,
                Location = src.Location,
                Pauses = (src.Pauses ?? new()).Where(p => p.End.HasValue && p.End.Value >= p.Start).ToList(),
                UserId = src.UserId,
                Username = string.IsNullOrWhiteSpace(src.Username) ? string.Empty : src.Username
                // WorkDurationNet / TotalPauseDuration sont calculées ou dérivées : on laisse null / non envoyées
            };

            if (clone.EndTime.HasValue && clone.EndTime.Value < clone.StartTime)
            {
                // Si incohérent, mieux vaut ne pas envoyer EndTime du tout
                clone.EndTime = null;
            }

            return clone;
        }

        private static async Task<string> SafeReadAsync(HttpResponseMessage res)
        {
            try { return await res.Content.ReadAsStringAsync().ConfigureAwait(false); }
            catch { return "<no-content>"; }
        }

        /// <summary>
        /// Log détaillé pour dev et message générique pour l'UI.
        /// </summary>
        private async Task<string> MapErrorAsync(HttpResponseMessage res, string scope)
        {
            string body = await SafeReadAsync(res).ConfigureAwait(false);
            int code = (int)res.StatusCode;

            _logger.LogWarning("{Scope} HTTP {Status} - {Body}", scope, code, body);

            return res.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                    => "Identifiants invalides ou autorisation insuffisante.",
                HttpStatusCode.NotFound
                    => "Service momentanément indisponible. Réessayez plus tard.",
                HttpStatusCode.BadRequest
                    => "Requête invalide. Vérifiez les informations saisies.",
                >= HttpStatusCode.MultipleChoices and <= (HttpStatusCode)399
                    => "Redirection détectée. Vérifiez la configuration réseau.",
                >= HttpStatusCode.InternalServerError and <= (HttpStatusCode)599
                    => "Erreur côté serveur. Réessayez plus tard.",
                _ => "Une erreur est survenue. Réessayez."
            };
        }
    }
}