// TimeTracker.Mobile/Services/MobileTimeEntryService.cs
#nullable enable
using System.Net.Http.Json;
using System.Text.Json;
using TimeTracker.Core.DTOs;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Service mobile pour gérer le cycle de vie de la session (en cours, sauvegarde locale, synchro API).
    /// - DTO-only (aucune entité EF côté mobile).
    /// - Persiste la session en cours en local (ILocalStorageService).
    /// - Synchronise avec l’API via HttpClient "Api" (Bearer ajouté par AuthHeaderHandler).
    /// </summary>
    public sealed class MobileTimeEntryService : IMobileTimeEntryService
    {
        private const string StorageKey = "InProgressSession";
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILocalStorageService _storage;

        // État en mémoire
        private TimeEntryDto? _inProgressSession;
        public TimeEntryDto? InProgressSession => _inProgressSession;

        private static class Api
        {
            public const string ClientName = "Api";                   // enregistré dans DI
            public const string TimeEntries = "api/timeentries";
            public static string ByUser(int userId) => $"api/timeentries?userId={userId}";
            public static string ById(int id) => $"api/timeentries/{id}";
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public MobileTimeEntryService(
            IHttpClientFactory httpFactory,
            ILocalStorageService storage)
        {
            _httpFactory = httpFactory;
            _storage = storage;
        }

        public async Task LoadInProgressSessionAsync()
        {
            var json = await _storage.GetStringAsync(StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                _inProgressSession = null;
                return;
            }

            try
            {
                _inProgressSession = JsonSerializer.Deserialize<TimeEntryDto>(json, JsonOpts);
            }
            catch
            {
                await _storage.RemoveAsync(StorageKey); // purge si corruption
                _inProgressSession = null;
            }
        }

        public async Task StartSessionAsync(TimeEntryDto dto)
        {
            _inProgressSession = dto;
            await SaveLocalAsync();
        }

        public async Task EndAndSaveCurrentSessionAsync()
        {
            if (_inProgressSession is null)
                throw new InvalidOperationException("Aucune session en cours.");

            if (_inProgressSession.Id <= 0)
                throw new InvalidOperationException("La session n’a pas encore été créée côté serveur.");

            var client = CreateClient();
            var resp = await client.PutAsJsonAsync(Api.ById(_inProgressSession.Id), _inProgressSession, JsonOpts);
            resp.EnsureSuccessStatusCode();

            _inProgressSession = null;
            await _storage.RemoveAsync(StorageKey);
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            var client = CreateClient();
            var list = await client.GetFromJsonAsync<IEnumerable<TimeEntryDto>>(Api.ByUser(userId), JsonOpts);
            return list ?? Enumerable.Empty<TimeEntryDto>();
        }

        public async Task CreateTimeEntryAsync(TimeEntryDto entry)
        {
            var client = CreateClient();
            var resp = await client.PostAsJsonAsync(Api.TimeEntries, entry, JsonOpts);
            resp.EnsureSuccessStatusCode();

            // Récupère l’entité créée (Id serveur, etc.)
            var created = await resp.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpts)
                          ?? throw new InvalidOperationException("Réponse serveur inattendue (TimeEntryDto nul).");

            entry.Id = created.Id;
            entry.UserId = created.UserId;
            entry.Username = created.Username ?? entry.Username;

            // Si c'est la même instance que la session en cours, on persiste l'Id
            if (ReferenceEquals(entry, _inProgressSession))
                await SaveLocalAsync();
        }

        // ----------------- Helpers -----------------
        private async Task SaveLocalAsync()
        {
            var json = JsonSerializer.Serialize(_inProgressSession, JsonOpts);
            await _storage.SetStringAsync(StorageKey, json);
        }

        private HttpClient CreateClient() => _httpFactory.CreateClient(Api.ClientName);
    }
}
