using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using WebUI.Core.Configuration;
using WebUI.Core.Security;
using Xunit;

namespace WebUI.Tests.Security
{
    public class JwtTokenServiceTests
    {
        private readonly IJwtTokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                SecretKey = "test-secret-key-minimum-32-characters-long",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            var options = Options.Create(_jwtSettings);
            _tokenService = new JwtTokenService(options);
        }

        [Fact]
        public void GenerateTokens_ShouldReturnValidTokenResult()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";

            // Act
            var result = _tokenService.GenerateTokens(userId, username);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            result.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void GenerateTokens_ShouldIncludeUserClaims()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";

            // Act
            var result = _tokenService.GenerateTokens(userId, username);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            // Assert
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
            token.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
            token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == username);
        }

        [Fact]
        public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";
            var tokenResult = _tokenService.GenerateTokens(userId, username);

            // Act
            var principal = _tokenService.ValidateToken(tokenResult.AccessToken);

            // Assert
            principal.Should().NotBeNull();
            principal!.Identity!.IsAuthenticated.Should().BeTrue();
            principal.FindFirst(ClaimTypes.Name)!.Value.Should().Be(username);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsNullOrEmpty(string token)
        {
            // Act
            var principal = _tokenService.ValidateToken(token);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var principal = _tokenService.ValidateToken(invalidToken);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnUniqueTokens()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            token1.Should().NotBeNullOrEmpty();
            token2.Should().NotBeNullOrEmpty();
            token1.Should().NotBe(token2);
        }

        [Fact]
        public void GenerateTokens_ShouldIncludeAdditionalClaims()
        {
            // Arrange
            var userId = 1;
            var username = "testuser";
            var additionalClaims = new System.Collections.Generic.Dictionary<string, string>
            {
                { "role", "admin" },
                { "email", "test@example.com" }
            };

            // Act
            var result = _tokenService.GenerateTokens(userId, username, additionalClaims);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            // Assert
            token.Claims.Should().Contain(c => c.Type == "role" && c.Value == "admin");
            token.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        }
    }
}
