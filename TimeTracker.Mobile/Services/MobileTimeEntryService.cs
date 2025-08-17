#nullable enable
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;

namespace TimeTracker.Mobile.Services
{
    /// <summary>
    /// Mobile service for time-entry lifecycle (in-progress state, local persistence, API sync).
    /// - DTO-only (no EF on mobile).
    /// - Persists in-progress session locally (ILocalStorageService).
    /// - Sends to API only when the session is completed (Bearer via AuthHeaderHandler).
    /// </summary>
    public sealed class MobileTimeEntryService : IMobileTimeEntryService
    {
        private const string StorageKey = "InProgressSession";

        private readonly IHttpClientFactory _httpFactory;
        private readonly ILocalStorageService _storage;

        // In-memory state
        private TimeEntryDto? _inProgressSession;
        public TimeEntryDto? InProgressSession => _inProgressSession;

        private static class Api
        {
            public const string ClientName = "Api";
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

        /// <summary>Load in-progress session from local storage into memory. Safe if key is missing or corrupted.</summary>
        public async Task LoadInProgressSessionAsync()
        {
            var json = await _storage.GetStringAsync(StorageKey).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                _inProgressSession = null;
                return;
            }

            try
            {
                _inProgressSession = JsonSerializer.Deserialize<TimeEntryDto>(json, JsonOpts);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"[MobileTimeEntryService] Corrupted local JSON. Purging key. Error: {ex}");
#endif
                await _storage.RemoveAsync(StorageKey).ConfigureAwait(false);
                _inProgressSession = null;
            }
        }

        /// <summary>Start a new local session (kept in memory + persisted locally). No network call.</summary>
        public async Task StartSessionAsync(TimeEntryDto dto)
        {
            _inProgressSession = dto;
            await SaveLocalAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Finalize current session: compute net duration, POST to API for creation, PUT only for admin update; on success clear local; on failure keep local so user can retry.
        /// </summary>
        public async Task EndAndSaveCurrentSessionAsync()
        {
            if (_inProgressSession is null)
                throw new InvalidOperationException("Aucune session en cours.");

            var s = _inProgressSession;

            if (!s.EndTime.HasValue)
                throw new InvalidOperationException("EndTime doit être renseigné avant l'enregistrement.");

            // 1) Raw duration
            var brut = s.EndTime.Value - s.StartTime;
            if (brut < TimeSpan.Zero) brut = TimeSpan.Zero;

            // 2) Closed pauses
            var pauses = s.Pauses ?? new List<PausePeriodDto>();
            var closedSeconds = pauses.Where(p => p.End.HasValue)
                                      .Sum(p => Math.Max(0, (p.End.Value - p.Start).TotalSeconds));
            // 3) Open pauses counted until EndTime
            var openSeconds = pauses.Where(p => !p.End.HasValue && p.Start < s.EndTime.Value)
                                    .Sum(p => Math.Max(0, (s.EndTime.Value - p.Start).TotalSeconds));

            var totalPauses = TimeSpan.FromSeconds(closedSeconds + openSeconds);
            var net = brut - totalPauses;
            if (net < TimeSpan.Zero) net = TimeSpan.Zero;

            s.WorkDurationNet = net;

            var client = CreateClient();
            try
            {
                HttpResponseMessage resp;
                if (s.Id <= 0)
                {
                    // Création (POST)
                    resp = await client.PostAsJsonAsync(Api.TimeEntries, s, JsonOpts).ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode)
                    {
#if DEBUG
                        var payload = JsonSerializer.Serialize(s, JsonOpts);
                        var body = resp.Content is null ? null : await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Debug.WriteLine($"[CreateEntry] FAILED {(int)resp.StatusCode} {resp.ReasonPhrase}\nPayload: {payload}\nResponse: {body ?? "<none>"}");
#endif
                        resp.EnsureSuccessStatusCode();
                    }

                    var created = await resp.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpts).ConfigureAwait(false)
                                  ?? throw new InvalidOperationException("Réponse serveur inattendue (TimeEntryDto nul).");

                    s.Id = created.Id;
                    s.UserId = created.UserId;
                    s.Username = created.Username ?? s.Username;
                }
                else
                {
                    // MAJ (PUT) - only for admins!
                    resp = await client.PutAsJsonAsync(Api.ById(s.Id), s, JsonOpts).ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode)
                    {
#if DEBUG
                        var payload = JsonSerializer.Serialize(s, JsonOpts);
                        var body = resp.Content is null ? null : await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Debug.WriteLine($"[EndSession] FAILED {(int)resp.StatusCode} {resp.ReasonPhrase}\nPayload: {payload}\nResponse: {body ?? "<none>"}");
#endif
                        resp.EnsureSuccessStatusCode();
                    }
                }

                _inProgressSession = null;
                await _storage.RemoveAsync(StorageKey).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
#if DEBUG
                var payload = JsonSerializer.Serialize(s, JsonOpts);
                Debug.WriteLine($"[EndSession] EXCEPTION {ex.GetType().Name}: {ex.Message}\nPayload: {payload}");
#endif
                // Keep local so user can retry later
                throw;
            }
        }

        /// <summary>
        /// Create entry on server. Should only be called for completed sessions (with both StartTime and EndTime set).
        /// If this instance == in-progress, persist updated Id locally.
        /// </summary>
        public async Task CreateTimeEntryAsync(TimeEntryDto entry)
        {
            // Ensure only completed sessions are sent to the server
            if (!entry.EndTime.HasValue)
                throw new InvalidOperationException("Cannot create entry on server: EndTime must be set (session must be complete).");

            var client = CreateClient();
            try
            {
                var resp = await client.PostAsJsonAsync(Api.TimeEntries, entry, JsonOpts).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode)
                {
#if DEBUG
                    var payload = JsonSerializer.Serialize(entry, JsonOpts);
                    var body = resp.Content is null ? null : await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Debug.WriteLine($"[CreateEntry] FAILED {(int)resp.StatusCode} {resp.ReasonPhrase}\nPayload: {payload}\nResponse: {body ?? "<none>"}");
#endif
                    resp.EnsureSuccessStatusCode();
                }

                var created = await resp.Content.ReadFromJsonAsync<TimeEntryDto>(JsonOpts).ConfigureAwait(false)
                              ?? throw new InvalidOperationException("Réponse serveur inattendue (TimeEntryDto nul).");

                entry.Id = created.Id;
                entry.UserId = created.UserId;
                entry.Username = created.Username ?? entry.Username;

                if (ReferenceEquals(entry, _inProgressSession))
                    await SaveLocalAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
#if DEBUG
                var payload = JsonSerializer.Serialize(entry, JsonOpts);
                Debug.WriteLine($"[CreateEntry] EXCEPTION {ex.GetType().Name}: {ex.Message}\nPayload: {payload}");
#endif
                throw;
            }
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            var client = CreateClient();
            var list = await client.GetFromJsonAsync<IEnumerable<TimeEntryDto>>(Api.ByUser(userId), JsonOpts)
                                  .ConfigureAwait(false);
            return list ?? Enumerable.Empty<TimeEntryDto>();
        }

        // ----------------- Helpers -----------------
        private async Task SaveLocalAsync()
        {
            if (_inProgressSession is null)
            {
                await _storage.RemoveAsync(StorageKey).ConfigureAwait(false);
                return;
            }

            var json = JsonSerializer.Serialize(_inProgressSession, JsonOpts);
            await _storage.SetStringAsync(StorageKey, json).ConfigureAwait(false);
        }

        private HttpClient CreateClient() => _httpFactory.CreateClient(Api.ClientName);
    }
}