using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using TimeTracker.API.Controllers;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Resources;
using Xunit;

namespace TimeTracker.Infrastructure.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IStringLocalizer<Errors>> _localizerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _localizerMock = new Mock<IStringLocalizer<Errors>>();
            _localizerMock.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));

            // Mock UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            _controller = new AuthController(
                _authServiceMock.Object,
                _localizerMock.Object,
                _userManagerMock.Object // <-- Ajouté ici
            );
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk()
        {
            // Arrange
            var dto = new ForgotPasswordRequestDto { Email = "user@example.com" };
            _authServiceMock.Setup(s => s.SendForgotPasswordEmailAsync(dto)).ReturnsAsync(true);

            // Act
            var result = await _controller.ForgotPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("PasswordResetEmailSent", okResult.Value.ToString());
        }

        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var dto = new ResetPasswordRequestDto
            {
                Email = "user@example.com",
                Token = "token",
                NewPassword = "NewPassword123!"
            };
            _authServiceMock.Setup(s => s.ResetPasswordAsync(dto)).ReturnsAsync(true);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("PasswordResetSuccess", okResult.Value.ToString());
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            var dto = new ResetPasswordRequestDto
            {
                Email = "user@example.com",
                Token = "token",
                NewPassword = "NewPassword123!"
            };
            _authServiceMock.Setup(s => s.ResetPasswordAsync(dto)).ReturnsAsync(false);

            // Act
            var result = await _controller.ResetPassword(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorResponseDto>(badRequestResult.Value);
            Assert.Equal("ResetFailed", error.Code);
        }
    }
}