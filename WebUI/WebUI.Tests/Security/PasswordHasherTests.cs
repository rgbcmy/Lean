using System;
using FluentAssertions;
using WebUI.Core.Security;
using Xunit;

namespace WebUI.Tests.Security
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void HashPassword_ShouldReturnValidHash()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hash = _passwordHasher.HashPassword(password);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().Contain(".");
            var parts = hash.Split('.');
            parts.Should().HaveCount(2);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashForSamePassword()
        {
            // Arrange
            var password = "TestPassword123!";

            // Act
            var hash1 = _passwordHasher.HashPassword(password);
            var hash2 = _passwordHasher.HashPassword(password);

            // Assert
            hash1.Should().NotBe(hash2, "salts should be different");
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
        {
            // Arrange
            var password = "TestPassword123!";
            var hash = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(password, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
        {
            // Arrange
            var password = "TestPassword123!";
            var wrongPassword = "WrongPassword123!";
            var hash = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void HashPassword_ShouldThrowException_WhenPasswordIsNullOrEmpty(string password)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(password));
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
        {
            // Arrange
            var password = "TestPassword123!";
            var invalidHash = "invalid-hash";

            // Act
            var result = _passwordHasher.VerifyPassword(password, invalidHash);

            // Assert
            result.Should().BeFalse();
        }
    }
}
