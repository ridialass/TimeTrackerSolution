using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using TimeTracker.Core.Resources;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;

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

        public TimeEntriesController(
            ApplicationDbContext db,
            IMapper mapper,
            ITimeEntryService timeEntryService, 
            IStringLocalizer<Errors> localizer
        )
        {
            _db = db;
            _mapper = mapper;
            _timeEntryService = timeEntryService;
            _localizer = localizer;
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
                    return Unauthorized();
                }
                userId = int.Parse(currentUserId);
            }

            if (userId.HasValue)
            {
                var list = await _timeEntryService.GetTimeEntriesByUserAsync(userId.Value);
                return Ok(list); // 200 OK
            }
            else
            {
                var list = await _timeEntryService.GetAllTimeEntriesAsync();
                return Ok(list); // 200 OK
            }
        }

        // GET: api/TimeEntries/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var te = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (te == null) return NotFound(); // 404

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && te.UserId.ToString() != currentUserId)
                return Forbid(); // 403

            return Ok(te); // 200 OK
        }

        // POST: api/TimeEntries
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400

            var created = await _timeEntryService.AddTimeEntryAsync(dto);

            return CreatedAtAction(nameof(GetById),
                                   new { id = created.Id },
                                   created); // 201 Created
        }

        // PUT: api/TimeEntries/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] TimeEntryDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch."); // 400

            dto.IsAdminModified = true;

            var updated = await _timeEntryService.UpdateTimeEntryAsync(dto);
            if (!updated)
                return NotFound(); // 404

            return NoContent(); // 204
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
                return NotFound(); // 404

            return NoContent(); // 204
        }

        // DELETE: api/TimeEntries/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _timeEntryService.DeleteTimeEntryAsync(id);
            if (!deleted) return NotFound(); // 404
            return NoContent(); // 204
        }
    }
}