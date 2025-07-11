using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using TimeTracker.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TimeTracker.Infrastructure.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAll_Should_Not_Expose_Sensitive_Fields()
        {
            // Arrange
            var client = _factory.CreateClient();
            // (Ajoute ici un setup d’authentification ou de seed si nécessaire)

            // Act
            var response = await client.GetAsync("/api/employees");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();

            // Assert: Ne doit contenir aucun champ sensible
            json.Should().NotContain("passwordHash", because: "le mot de passe ne doit jamais être exposé");
            json.Should().NotContain("securityStamp");
            json.Should().NotContain("concurrencyStamp");
            json.Should().NotContain("emailConfirmed");
            json.Should().NotContain("phoneNumber");
            json.Should().NotContain("phoneNumberConfirmed");
            json.Should().NotContain("twoFactorEnabled");
            json.Should().NotContain("lockoutEnabled");
            json.Should().NotContain("lockoutEnd");
            json.Should().NotContain("accessFailedCount");
        }

        [Fact]
        public async Task GetById_Should_Return_Only_Allowed_Fields()
        {
            // Arrange
            var client = _factory.CreateClient();
            // (Ajoute ici un setup d’authentification ou de seed si nécessaire)
            var userId = 1; // à adapter selon tes données de test

            // Act
            var response = await client.GetAsync($"/api/employees/{userId}");
            response.StatusCode.Should().Match(x => x == HttpStatusCode.OK || x == HttpStatusCode.NotFound);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();

                // Assert: Ne doit contenir aucun champ sensible
                json.Should().NotContain("passwordHash");
                json.Should().NotContain("securityStamp");
                json.Should().NotContain("concurrencyStamp");
                json.Should().NotContain("emailConfirmed");
                json.Should().NotContain("phoneNumber");
                json.Should().NotContain("phoneNumberConfirmed");
                json.Should().NotContain("twoFactorEnabled");
                json.Should().NotContain("lockoutEnabled");
                json.Should().NotContain("lockoutEnd");
                json.Should().NotContain("accessFailedCount");
            }
        }
    }
}

