using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public EmployeeController(IEmployeeService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllEmployeesAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var emp = await _service.GetEmployeeByIdAsync(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteEmployeeAsync(id);
            if (!deleted) return NotFound(new { message = "Employee not found" });
            return NoContent();
        }

        // (Optionally) implement Update, etc.
    }
}
