using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // all authenticated users can use these endpoints
    public class TimeEntryController : ControllerBase
    {
        private readonly ITimeEntryService _timeEntryService;
        public TimeEntryController(ITimeEntryService timeEntryService)
            => _timeEntryService = timeEntryService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _timeEntryService.GetAllTimeEntriesAsync();
            return Ok(list);
        }

        [HttpGet("ApplicationUser/{ApplicationUserId:int}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var list = await _timeEntryService.GetTimeEntriesByEmployeeAsync(employeeId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TimeEntryDto newEntry)
        {
            var created = await _timeEntryService.CreateTimeEntryAsync(newEntry);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var te = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (te == null) return NotFound();
            return Ok(te);
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
