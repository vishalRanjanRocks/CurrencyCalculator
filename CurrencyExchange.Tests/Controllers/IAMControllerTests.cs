using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CurrencyExchange.Controllers;
using CurrencyExchange.Infrastructure.Services;
using CurrencyExchange.Infrastructure.Models;
using CurrencyExchange.Models;
using Microsoft.Extensions.Options;

namespace CurrencyExchange.Tests.Controllers
{
    public class IAMControllerTests
    {
        private readonly AuthService _authService;
        private readonly IAMController _controller;

        public IAMControllerTests()
        {
            // Mock the AuthService (you'll need to mock its dependencies too)
            var jwtSettings = Options.Create(new JwtSettings
            {
                Issuer = "test",
                Audience = "test"
            });

            // Create real AuthService
            _authService = new AuthService(jwtSettings);

            // Setup the controller with mocked service
            _controller = new IAMController(_authService);
        }

        [Theory]
        [InlineData(null, "Admin", "password")]
        [InlineData("", "Admin", "password")]
        [InlineData("username", "", "password")]
        [InlineData("username", "Admin", "")]
        public void Login_ReturnsBadRequest_WhenFieldsMissing(string username, string role, string password)
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = username,
                Role = role,
                Password = password
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Invalid login request", badRequestResult.Value);
        }

        [Fact]
        public void Login_ReturnsUnauthorized_WhenPasswordIncorrect()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "testuser",
                Role = "User",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

    }
}