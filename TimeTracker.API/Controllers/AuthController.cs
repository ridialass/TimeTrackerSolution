using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;
using TimeTracker.API.Resources;

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
                return BadRequest(ModelState);

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
                return BadRequest(ModelState);

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
                return BadRequest(ModelState);

            await _authService.SendForgotPasswordEmailAsync(dto);
            return Ok(new { message = _localizer["PasswordResetEmailSent"] ?? "Si un compte existe pour cet email, un lien a été envoyé." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool result = await _authService.ResetPasswordAsync(dto);
            if (!result)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "ResetFailed",
                    Message = _localizer["PasswordResetFailed"] ?? "Impossible de réinitialiser le mot de passe. Vérifiez le token ou l'email."
                });
            }
            return Ok(new { message = _localizer["PasswordResetSuccess"] ?? "Mot de passe réinitialisé avec succès." });
        }

        // ÉTAPE 3 : Endpoint pour rafraîchir le token
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(dto);
            if (result == null)
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "InvalidRefreshToken",
                    Message = _localizer["InvalidRefreshToken"] ?? "Refresh token invalide ou expiré."
                });

            return Ok(result);
        }

        // ÉTAPE 2 : Endpoint pour changer le mot de passe
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "UserNotFound",
                    Message = "Utilisateur non trouvé."
                });

            var result = await _authService.ChangePasswordAsync(dto, username);
            if (!result)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Code = "ChangePasswordFailed",
                    Message = "Échec du changement de mot de passe. Vérifiez l'ancien mot de passe."
                });
            }
            return Ok(new { message = "Mot de passe changé avec succès." });
        }

        // ÉTAPE 1 : Endpoint pour envoyer 2FA
        [HttpPost("send-2fa-code")]
        [AllowAnonymous]
        public async Task<IActionResult> Send2FACode([FromBody] Send2FACodeRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Send2FACodeAsync(dto);
            if (!result)
                return BadRequest(new ErrorResponseDto
                {
                    Code = "Send2FACodeFailed",
                    Message = "Impossible d'envoyer le code 2FA."
                });

            return Ok(new { message = "Code 2FA envoyé avec succès." });
        }

        // ÉTAPE 5 : Endpoint pour vérifier le code 2FA
        [HttpPost("verify-2fa-code")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify2FACode([FromBody] Verify2FACodeRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Verify2FACodeAsync(dto);
            if (!result)
                return Unauthorized(new ErrorResponseDto
                {
                    Code = "Invalid2FACode",
                    Message = "Code 2FA invalide ou expiré."
                });

            return Ok(new { message = "Code 2FA vérifié avec succès." });
        }

        // ÉTAPE 4 : Endpoint protégé pour admin (exemple)
        [HttpGet("protected")]
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult ProtectedAdminEndpoint()
        {
            return Ok("Vous êtes authentifié en tant qu’Admin !");
        }
    }
}