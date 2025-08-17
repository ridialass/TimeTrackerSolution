using System.Diagnostics;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Mobile.Services.Interfaces;
using TimeTracker.Mobile.Utils;

namespace TimeTracker.Mobile.Services
{
    public sealed class MobileTimeEntryService : IMobileTimeEntryService
    {
        private const string StorageKey = "InProgressSession";
        private readonly IApiClientService _apiClient;
        private readonly ILocalStorageService _storage;

        // In-memory state
        private TimeEntryDto? _inProgressSession;
        public TimeEntryDto? InProgressSession => _inProgressSession;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public MobileTimeEntryService(
            IApiClientService apiClient,
            ILocalStorageService storage)
        {
            _apiClient = apiClient;
            _storage = storage;
        }

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

        public async Task StartSessionAsync(TimeEntryDto dto)
        {
            _inProgressSession = dto;
            await SaveLocalAsync().ConfigureAwait(false);
        }

        public async Task EndAndSaveCurrentSessionAsync()
        {
            if (_inProgressSession is null)
                throw new InvalidOperationException("Aucune session en cours.");

            var s = _inProgressSession;

            if (!s.EndTime.HasValue)
                throw new InvalidOperationException("EndTime doit être renseigné avant l'enregistrement.");

            // Duration/net time logic remains unchanged
            var brut = s.EndTime.Value - s.StartTime;
            if (brut < TimeSpan.Zero) brut = TimeSpan.Zero;

            var pauses = s.Pauses ?? new List<PausePeriodDto>();
            var closedSeconds = pauses.Where(p => p.End.HasValue)
                                      .Sum(p => Math.Max(0, (p.End.Value - p.Start).TotalSeconds));
            var openSeconds = pauses.Where(p => !p.End.HasValue && p.Start < s.EndTime.Value)
                                    .Sum(p => Math.Max(0, (s.EndTime.Value - p.Start).TotalSeconds));
            var totalPauses = TimeSpan.FromSeconds(closedSeconds + openSeconds);
            var net = brut - totalPauses;
            if (net < TimeSpan.Zero) net = TimeSpan.Zero;
            s.WorkDurationNet = net;

            // Save via API client : uniquement POST (création)
            var result = await _apiClient.CreateTimeEntryAsync(s);

            if (!result.IsSuccess)
                throw new Exception(result.Error);

            // Optionally update local session (Id/UserId/Username) with response
            if (result.Value != null)
            {
                s.Id = result.Value.Id;
                s.UserId = result.Value.UserId;
                s.Username = result.Value.Username ?? s.Username;
            }

            _inProgressSession = null;
            await _storage.RemoveAsync(StorageKey).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int userId)
        {
            var result = await _apiClient.GetTimeEntriesAsync(userId).ConfigureAwait(false);
            return result.IsSuccess && result.Value != null
                ? result.Value
                : Enumerable.Empty<TimeEntryDto>();
        }

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
    }
}