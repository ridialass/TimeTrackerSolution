using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Resources;

namespace TimeTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<Errors> _localizer;

        public AuthController(IAuthService authService, IStringLocalizer<Errors> localizer)
        {
            _authService = authService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            try
            {
                var result = await _authService.AuthenticateAsync(model);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "InvalidCredentials",
                    Message = _localizer["InvalidCredentials"]
                });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            bool created = await _authService.RegisterAsync(model);
            if (!created)
                return Conflict(new ErrorResponseDto
                {
                    Code = "DuplicateUserOrEmail",
                    Message = _localizer["DuplicateUserOrEmail"]
                });

            return Ok(new { username = model.Username });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            await _authService.SendForgotPasswordEmailAsync(dto);
            return Ok(new { message = _localizer["PasswordResetEmailSent"] });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            bool result = await _authService.ResetPasswordAsync(dto);
            if (!result)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "ResetFailed",
                    Message = _localizer["PasswordResetFailed"]
                });
            }
            return Ok(new { message = _localizer["PasswordResetSuccess"] });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            var result = await _authService.RefreshTokenAsync(dto);
            if (result == null)
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "InvalidRefreshToken",
                    Message = _localizer["InvalidRefreshToken"]
                });

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "UserNotFound",
                    Message = _localizer["UserNotFound"]
                });

            var result = await _authService.ChangePasswordAsync(dto, username);
            if (!result)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "ChangePasswordFailed",
                    Message = _localizer["ChangePasswordFailed"]
                });
            }
            return Ok(new { message = _localizer["ChangePasswordSuccess"] });
        }

        [HttpPost("send-2fa-code")]
        [AllowAnonymous]
        public async Task<IActionResult> Send2FACode([FromBody] Send2FACodeRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            var result = await _authService.Send2FACodeAsync(dto);
            if (!result)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "Send2FACodeFailed",
                    Message = _localizer["Send2FACodeFailed"]
                });

            return Ok(new { message = _localizer["Send2FACodeSuccess"] });
        }

        [HttpPost("verify-2fa-code")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FACode([FromBody] Verify2FACodeRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "InvalidModel",
                    Message = _localizer["InvalidModel"]
                });

            var result = await _authService.Verify2FACodeAsync(dto);
            if (!result)
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "Invalid2FACode",
                    Message = _localizer["Invalid2FACode"]
                });

            return Ok(new { message = _localizer["Verify2FACodeSuccess"] });
        }

        [HttpGet("protected")]
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult ProtectedAdminEndpoint()
        {
            return Ok(_localizer["AuthenticatedAsAdmin"]);
        }
    }
}