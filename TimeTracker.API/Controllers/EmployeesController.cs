using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TimeTracker.Core.Resources;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;
using TimeTracker.Core.Helpers;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly IStringLocalizer<Errors> _localizer;
        private readonly IStringLocalizer<EnumLabels> _enumLocalizer;

        public EmployeesController(
            IEmployeeService service,
            IStringLocalizer<Errors> localizer,
            IStringLocalizer<EnumLabels> enumLocalizer
        )
        {
            _service = service;
            _localizer = localizer;
            _enumLocalizer = enumLocalizer;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllEmployeesAsync();
            Console.WriteLine("Nombre d'employés retournés : " + list.Count());
            return Ok(list);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var list = await _service.GetAllEmployeesAsync();

        //    // Ajoute le label localisé du rôle à chaque employé
        //    var result = list.Select(emp => new
        //    {
        //        emp.Id,
        //        emp.FirstName,
        //        emp.LastName,
        //        emp.Email,
        //        emp.Role,
        //        UserRoleLabel = EnumLocalizationHelper.GetEnumLabel(emp.Role, _enumLocalizer),
        //        // Ajoute ici les autres propriétés du DTO si besoin
        //    });

        //    return Ok(result);
        //}

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
            return Ok(new { message = _localizer["EmployeeDeleted"] });
        }

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

            return Ok(new { message = _localizer["EmployeeUpdated"] ?? "Employé modifié avec succès" });
        }

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

            return Ok(new { message = _localizer["EmployeePatched"] ?? "Employé mis à jour partiellement avec succès" });
        }

        [HttpGet("paged")]
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