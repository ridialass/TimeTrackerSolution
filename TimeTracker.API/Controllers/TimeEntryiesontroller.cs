using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using TimeTracker.Core.Resources;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ITimeEntryService _timeEntryService;
        private readonly IStringLocalizer<Errors> _localizer;
        private readonly IStringLocalizer<EnumLabels> _enumLocalizer;

        public TimeEntriesController(
            ApplicationDbContext db,
            IMapper mapper,
            ITimeEntryService timeEntryService,
            IStringLocalizer<Errors> localizer,
            IStringLocalizer<EnumLabels> enumLocalizer
        )
        {
            _db = db;
            _mapper = mapper;
            _timeEntryService = timeEntryService;
            _localizer = localizer;
            _enumLocalizer = enumLocalizer;
        }

        // GET: api/TimeEntries?userId=xx
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ErrorResponseDto
                    {
                        Code = "Unauthorized",
                        Message = _localizer["Unauthorized"]
                    });
                }
                userId = int.Parse(currentUserId);
            }

            IEnumerable<TimeEntryDto> list;
            if (userId.HasValue)
                list = await _timeEntryService.GetTimeEntriesByUserAsync(userId.Value);
            else
                list = await _timeEntryService.GetAllTimeEntriesAsync();

            // Ajoute le label localisé pour SessionType et DinnerPaidBy à chaque item
            var result = list.Select(te => new
            {
                te.Id,
                te.StartTime,
                te.EndTime,
                te.UserId,
                te.Username,
                te.SessionType,
                SessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(te.SessionType, _enumLocalizer),
                te.IsAdminModified,
                te.IncludesTravelTime,
                te.DinnerPaid,
                DinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(te.DinnerPaid, _enumLocalizer),
                te.Location,
                // autres propriétés...
            });

            return Ok(result);
        }

        // GET: api/TimeEntries/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var te = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (te == null)
                return NotFound(new ErrorResponseDto
                {
                    Code = "TimeEntryNotFound",
                    Message = _localizer["TimeEntryNotFound"]
                });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && te.UserId.ToString() != currentUserId)
                return StatusCode(403, new ErrorResponseDto
                {
                    Code = "Forbidden",
                    Message = _localizer["Forbidden"]
                });

            var result = new
            {
                te.Id,
                te.StartTime,
                te.EndTime,
                te.UserId,
                te.Username,
                te.SessionType,
                SessionTypeLabel = EnumLocalizationHelper.GetEnumLabel(te.SessionType, _enumLocalizer),
                te.IsAdminModified,
                te.IncludesTravelTime,
                te.DinnerPaid,
                DinnerPaidLabel = EnumLocalizationHelper.GetEnumLabel(te.DinnerPaid, _enumLocalizer),
                te.Location,
                // autres propriétés...
            };

            return Ok(result);
        }

        // GET: api/TimeEntries/enum-labels
        [HttpGet("enum-labels")]
        [AllowAnonymous]
        public IActionResult GetEnumLabels()
        {
            var workSessionTypeLabels = Enum.GetValues(typeof(WorkSessionType))
                .Cast<WorkSessionType>()
                .ToDictionary(
                    e => (int)e,
                    e => EnumLocalizationHelper.GetEnumLabel(e, _enumLocalizer)
                );

            var dinnerPaidByLabels = Enum.GetValues(typeof(DinnerPaidBy))
                .Cast<DinnerPaidBy>()
                .ToDictionary(
                    e => (int)e,
                    e => EnumLocalizationHelper.GetEnumLabel(e, _enumLocalizer)
                );

            var userRoleLabels = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .ToDictionary(
                    e => (int)e,
                    e => EnumLocalizationHelper.GetEnumLabel(e, _enumLocalizer)
                );

            return Ok(new
            {
                WorkSessionType = workSessionTypeLabels,
                DinnerPaidBy = dinnerPaidByLabels,
                UserRole = userRoleLabels
            });
        }

        // POST: api/TimeEntries
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            var created = await _timeEntryService.AddTimeEntryAsync(dto);

            return CreatedAtAction(nameof(GetById),
                                   new { id = created.Id },
                                   created);
        }

        // PUT: api/TimeEntries/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] TimeEntryDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "IdMismatch",
                    Message = _localizer["IdMismatch"]
                });

            dto.IsAdminModified = true;

            var updated = await _timeEntryService.UpdateTimeEntryAsync(dto);
            if (!updated)
                return NotFound(new ErrorResponseDto
                {
                    Code = "TimeEntryNotFound",
                    Message = _localizer["TimeEntryNotFound"]
                });

            return Ok(new { message = _localizer["TimeEntryUpdated"] });
        }

        // PATCH: api/TimeEntries/{id}
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Patch(int id, [FromBody] TimeEntryDto dto)
        {
            dto.Id = id;
            dto.IsAdminModified = true;

            var updated = await _timeEntryService.UpdateTimeEntryAsync(dto);
            if (!updated)
                return NotFound(new ErrorResponseDto
                {
                    Code = "TimeEntryNotFound",
                    Message = _localizer["TimeEntryNotFound"]
                });

            return Ok(new { message = _localizer["TimeEntryPatched"] });
        }

        // DELETE: api/TimeEntries/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _timeEntryService.DeleteTimeEntryAsync(id);
            if (!deleted)
                return NotFound(new ErrorResponseDto
                {
                    Code = "TimeEntryNotFound",
                    Message = _localizer["TimeEntryNotFound"]
                });
            return Ok(new { message = _localizer["TimeEntryDeleted"] });
        }
    }
}