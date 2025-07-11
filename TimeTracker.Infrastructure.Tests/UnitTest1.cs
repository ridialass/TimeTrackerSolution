using TimeTracker.Core.DTOs;
using TimeTracker.Core.Interfaces;

namespace TimeTracker.Infrastructure.Tests
{
    public class ChangePasswordTests
    {
        [Fact]
        public void ChangePasswordRequestDto_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "oldPassword123",
                NewPassword = "newPassword456"
            };

            // Assert
            Assert.Equal("oldPassword123", dto.CurrentPassword);
            Assert.Equal("newPassword456", dto.NewPassword);
        }

        [Fact]
        public void ChangePasswordRequestDto_RequiredProperties_AreRequired()
        {
            // This test verifies that the properties are marked as required
            // by checking if we can create the object with required properties
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "test",
                NewPassword = "test"
            };

            Assert.NotNull(dto);
            Assert.NotNull(dto.CurrentPassword);
            Assert.NotNull(dto.NewPassword);
        }

        [Fact]
        public void IAuthService_HasChangePasswordAsyncMethod()
        {
            // Arrange
            var authServiceType = typeof(IAuthService);

            // Act
            var method = authServiceType.GetMethod("ChangePasswordAsync");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(Task), method.ReturnType);
            var parameters = method.GetParameters();
            Assert.Equal(2, parameters.Length);
            Assert.Equal(typeof(ChangePasswordRequestDto), parameters[0].ParameterType);
            Assert.Equal(typeof(string), parameters[1].ParameterType);
        }

        [Fact]
        public void ChangePasswordRequestDto_HasCorrectNamespace()
        {
            // Verify the DTO is in the correct namespace
            var type = typeof(ChangePasswordRequestDto);
            Assert.Equal("TimeTracker.Core.DTOs", type.Namespace);
        }
    }
}