using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TimeTracker.Core.Resources;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly IStringLocalizer<Errors> _localizer;

        public EmployeesController(IEmployeeService service, IStringLocalizer<Errors> localizer)
        {
            _service = service;
            _localizer = localizer;
        }

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
            if (emp == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = "EmployeeNotFound",
                    Message = _localizer["EmployeeNotFound"]
                });
            }
            return Ok(emp);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteEmployeeAsync(id);
            if (!deleted)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = "EmployeeNotFound",
                    Message = _localizer["EmployeeNotFound"]
                });
            }
            return NoContent();
        }

        // PUT: api/Employees/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            if (dto == null || id != dto.Id)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });
            }

            var updated = await _service.UpdateEmployeeAsync(dto);
            if (!updated)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = "EmployeeNotFound",
                    Message = _localizer["EmployeeNotFound"]
                });
            }

            return NoContent();
        }

        // PATCH: api/Employees/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] PatchEmployeeDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });
            }

            var patched = await _service.PatchEmployeeAsync(id, dto);
            if (!patched)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = "EmployeeNotFound",
                    Message = _localizer["EmployeeNotFound"]
                });
            }

            return NoContent();
        }

        // Pagination/filtrage
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] EmployeeQueryParameters query)
        {
            var (items, totalCount) = await _service.GetEmployeesPagedAsync(query);
            var result = new
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                PageCount = (int)Math.Ceiling((double)totalCount / query.PageSize)
            };
            return Ok(result);
        }
    }
}