using AutoMapper;
using FluentAssertions;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;
using TimeTracker.Core.Enums;
using TimeTracker.Infrastructure.Mapping;
using Xunit;

namespace TimeTracker.Infrastructure.Tests.Mapping
{
    public class SecurityMappingTests
    {
        private readonly IMapper _mapper;

        public SecurityMappingTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ApplicationMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void ApplicationUser_To_EmployeeDto_Should_Not_Expose_Sensitive_Fields()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = 42,
                UserName = "testuser",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Town = "Paris",
                Country = "France",
                Role = UserRole.Admin,
                PasswordHash = "sensitiveHash",
                SecurityStamp = "secret",
                ConcurrencyStamp = "anotherSecret",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = true,
                LockoutEnabled = true,
                LockoutEnd = DateTimeOffset.MaxValue,
                AccessFailedCount = 99
            };

            // Act
            var dto = _mapper.Map<EmployeeDto>(user);

            // Assert
            // Champs autorisés
            dto.Should().NotBeNull();
            dto.Id.Should().Be(user.Id);
            dto.Username.Should().Be(user.UserName);
            dto.FirstName.Should().Be(user.FirstName);
            dto.LastName.Should().Be(user.LastName);
            dto.Email.Should().Be(user.Email);
            dto.Town.Should().Be(user.Town);
            dto.Country.Should().Be(user.Country);
            dto.Role.Should().Be(user.Role);

            // Champs sensibles NE DOIVENT PAS SORTIR
            var dtoType = dto.GetType();
            dtoType.GetProperty("PasswordHash").Should().BeNull();
            dtoType.GetProperty("SecurityStamp").Should().BeNull();
            dtoType.GetProperty("ConcurrencyStamp").Should().BeNull();
            dtoType.GetProperty("EmailConfirmed").Should().BeNull();
            dtoType.GetProperty("PhoneNumber").Should().BeNull();
            dtoType.GetProperty("PhoneNumberConfirmed").Should().BeNull();
            dtoType.GetProperty("TwoFactorEnabled").Should().BeNull();
            dtoType.GetProperty("LockoutEnabled").Should().BeNull();
            dtoType.GetProperty("LockoutEnd").Should().BeNull();
            dtoType.GetProperty("AccessFailedCount").Should().BeNull();
        }
    }
}