using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Resources;
using TimeTracker.Infrastructure.Services;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Errors> _localizer;
        private readonly IStringLocalizer<EnumLabels> _enumLocalizer;

        public EmployeesController(
            IEmployeeService service,
            IStringLocalizer<Errors> localizer,
            IStringLocalizer<EnumLabels> enumLocalizer,
            UserManager<ApplicationUser> userManager
        )
        {
            _service = service;
            _userManager = userManager;
            _localizer = localizer;
            _enumLocalizer = enumLocalizer;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllEmployeesAsync();
            Console.WriteLine("Nombre d'employés retournés : " + list.Count());
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
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

            var result = new
            {
                emp.Id,
                emp.FirstName,
                emp.LastName,
                emp.Email,
                emp.Role,
                UserRoleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _enumLocalizer),
                // autres propriétés...
            };

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var profile = await _service.GetMyProfileAsync(userId);
            if (profile == null)
                return NotFound();
            return Ok(profile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.UpdateMyProfileAsync(userId, dto);
            if (!result)
                return BadRequest();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
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
            return Ok(new { message = _localizer["EmployeeDeleted"] });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
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

            return Ok(new { message = _localizer["EmployeeUpdated"] ?? "Employé modifié avec succès" });
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
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

            return Ok(new { message = _localizer["EmployeePatched"] ?? "Employé mis à jour partiellement avec succès" });
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaged([FromQuery] EmployeeQueryParameters query)
        {
            var (items, totalCount) = await _service.GetEmployeesPagedAsync(query);
            var result = new
            {
                Items = items.Select(emp => new
                {
                    emp.Id,
                    emp.FirstName,
                    emp.LastName,
                    emp.Email,
                    emp.Role,
                    RoleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _enumLocalizer),
                    // autres propriétés...
                }),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                PageCount = (int)Math.Ceiling((double)totalCount / query.PageSize)
            };
            return Ok(result);
        }

        // Route pour exposer tous les labels traduits des rôles (pour l'UI)
        [HttpGet("role-labels")]
        [AllowAnonymous]
        public IActionResult GetRoleLabels()
        {
            var labels = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .ToDictionary(
                    e => (int)e,
                    e => EnumLocalizationHelper.GetEnumLabel(e, _enumLocalizer)
                );
            return Ok(labels);
        }
    }
}