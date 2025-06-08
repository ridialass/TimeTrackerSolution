using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.Mobile.Services
{
    public class MobileTimeEntryService
    {
        private readonly IApiClientService _apiClient;
        private readonly MobileAuthService _authService;

        public MobileTimeEntryService(IApiClientService apiClient, MobileAuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        public async Task<TimeEntryDto> StartNewSessionAsync(WorkSessionPayload payload)
        {
            // Convert payload to TimeEntryDto
            var dto = new TimeEntryDto
            {
                StartTime = payload.StartTime,
                StartLatitude = payload.StartLatitude,
                StartLongitude = payload.StartLongitude,
                StartAddress = payload.StartAddress,
                SessionType = payload.SessionType,
                IncludesTravelTime = payload.IncludesTravelTime,
                Location = payload.StartAddress,
                UserId = _authService.CurrentUser!.EmployeeId,
                Username = _authService.CurrentUser.Username,
            };

            // Initially, EndTime=null; we’ll set EndTime when calling EndSessionAsync
            return dto;
        }

        public async Task<TimeEntryDto> EndAndSaveSessionAsync(TimeEntryDto inProgressDto, WorkSessionPayload payload)
        {
            // payload carries TravelDurationHours, EndTime, EndLatitude, EndLongitude, EndAddress, DinnerPaid
            inProgressDto.EndTime = payload.EndTime;
            inProgressDto.EndLatitude = payload.EndLatitude;
            inProgressDto.EndLongitude = payload.EndLongitude;
            inProgressDto.EndAddress = payload.EndAddress;
            inProgressDto.TravelDurationHours = payload.TravelDurationHours;
            inProgressDto.DinnerPaid = payload.DinnerPaid;

            var jwt = _authService.CurrentUser!.Token;
            var stored = await _apiClient.CreateTimeEntryAsync(inProgressDto, jwt);
            return stored;
        }

        public async Task<List<TimeEntryDto>> GetMySessionsAsync()
        {
            var jwt = _authService.CurrentUser!.Token;
            var myId = _authService.CurrentUser.EmployeeId;
            return await _apiClient.GetTimeEntriesByEmployeeAsync(myId, jwt);
        }
    }

    // a local payload to carry StartTime, location, etc., 
    // before the user “ends” the session
    public class WorkSessionPayload
    {
        public DateTime StartTime { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string StartAddress { get; set; } = default!;
        public WorkSessionType SessionType { get; set; }
        public bool IncludesTravelTime { get; set; }
        public string Location { get; set; } = default!;

        public DateTime EndTime { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public string EndAddress { get; set; } = default!;
        public double? TravelDurationHours { get; set; }
        public DinnerPaidBy DinnerPaid { get; set; }
    }
}
