using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyExchange.Infrastructure.Services;
using CurrencyExchange.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyExchange.Tests.Infrastructure.Services
{
    public class AuthServiceTests
    {
        private readonly JwtSettings _settings;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _settings = new JwtSettings
            {
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                Key = "ThisIsASecureKeyThatIsAtLeast32Chars!" // 256-bit key
            };

            var options = Options.Create(_settings);
            _authService = new AuthService(options);
        }

        [Fact]
        public void GenerateToken_ShouldReturn_ValidJwt_WithCorrectClaims()
        {
            // Arrange
            string username = "testuser";
            string role = "Admin";

            // Act
            var token = _authService.GenerateToken(username, role);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false, // not setting audience in token
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(token, parameters, out var validatedToken);
            var identity = principal.Identity as ClaimsIdentity;

            identity.Should().NotBeNull();
            identity?.IsAuthenticated.Should().BeTrue();
            identity?.FindFirst(ClaimTypes.Name)?.Value.Should().Be(username);
            identity?.FindFirst(ClaimTypes.Role)?.Value.Should().Be(role);
        }

        [Fact]
        public void GenerateToken_ShouldContain_ValidExpirationTime()
        {
            // Act
            var token = _authService.GenerateToken("expUser", "User");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
            (jwt.ValidTo - DateTime.UtcNow).TotalMinutes.Should().BeGreaterThan(59); // ~1 hour
        }

        [Fact]
        public void GenerateToken_ShouldThrow_WhenKeyIsTooShort()
        {
            // Arrange
            var badSettings = Options.Create(new JwtSettings
            {
                Issuer = "BadIssuer",
                Audience = "BadAudience",
                Key = "short" // ❌ too short
            });

            var badService = new AuthService(badSettings);

            // Act
            Action act = () => badService.GenerateToken("user", "Admin");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .WithMessage("*IDX10653: The encryption algorithm 'HS256' requires a key size of at least '128' bits. Key 'Null', is of size: '40'. (Parameter 'key')*");
        }
    }
}
