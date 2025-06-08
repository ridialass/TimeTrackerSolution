using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;
using TimeTracker.Infrastructure.Services;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
            => _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.AuthenticateAsync(model);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Identifiants invalides." });
            }
        }


        ////[AllowAnonymous] // Pour le setup initial, retire-le ensuite
        //[HttpPost("register")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Register(
        //[FromBody] RegisterRequestDto model)
        //{
        //    if (!ModelState.IsValid)
        //        // pour débugger, renvoyer la liste des erreurs
        //        return BadRequest(new { errors = ModelState });

        //    // On mappe manuellement (ou via AutoMapper si vous préférez) :
        //    var dto = new EmployeeDto
        //    {
        //        Username = model.Username,
        //        Role = model.Role,
        //        FirstName = model.FirstName,
        //        LastName = model.LastName,
        //        Town = model.Town,
        //        Country = model.Country
        //    };

        //    bool created = await _auth.RegisterAsync(dto, model.Password);
        //    if (!created)
        //        return Conflict(new { message = "Username already exists" });

        //    return Ok(new { username = model.Username });
        //}

        [AllowAnonymous]
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool created = await _authService.RegisterAsync(model);
            if (!created)
                return Conflict(new { message = "Nom d’utilisateur ou email déjà utilisé." });

            return Ok(new { username = model.Username });
        }

    }
}
