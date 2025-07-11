using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public TimeEntriesController(
            ApplicationDbContext db,
            IMapper mapper,
            ITimeEntryService timeEntryService
        )
        {
            _db = db;
            _mapper = mapper;
            _timeEntryService = timeEntryService;
        }

        // GET: api/TimeEntries?userId=xx
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId = null)
        {
            if (userId.HasValue)
            {
                var list = await _timeEntryService.GetTimeEntriesByUserAsync(userId.Value);
                return Ok(list);
            }
            else
            {
                var list = await _timeEntryService.GetAllTimeEntriesAsync();
                return Ok(list);
            }
        }

        // GET: api/TimeEntries/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var te = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (te == null) return NotFound();
            return Ok(te);
        }

        // POST: api/TimeEntries
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
                return BadRequest("ID mismatch.");

            dto.IsAdminModified = true; // Marquer la modif admin

            var updated = await _timeEntryService.UpdateTimeEntryAsync(dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // PATCH: api/TimeEntries/{id}
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Patch(int id, [FromBody] TimeEntryDto dto)
        {
            dto.Id = id;
            dto.IsAdminModified = true; // Marquer la modif admin

            var updated = await _timeEntryService.UpdateTimeEntryAsync(dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/TimeEntries/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _timeEntryService.DeleteTimeEntryAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}