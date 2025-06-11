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
    public class TimeEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ITimeEntryService _timeEntryService;    // ← on déclare le service

        public TimeEntryController(
            ApplicationDbContext db,
            IMapper mapper,
            ITimeEntryService timeEntryService   // ← on l’injecte
        )
        {
            _db = db;
            _mapper = mapper;
            _timeEntryService = timeEntryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _timeEntryService.GetAllTimeEntriesAsync();
            return Ok(list);
        }

        [HttpGet("ApplicationUser/{employeeId:int}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var list = await _timeEntryService.GetTimeEntriesByUserAsync(employeeId);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var te = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (te == null) return NotFound();
            return Ok(te);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeEntryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // AutoMapper transforme le DTO en entité
            var entity = _mapper.Map<TimeEntry>(dto);
            _db.TimeEntries.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                                   new { id = entity.Id },
                                   entity);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _timeEntryService.DeleteTimeEntryAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
