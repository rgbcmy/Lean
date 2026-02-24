using FluentAssertions;
using WebUI.Core.Security;
using Xunit;

namespace WebUI.Tests.Security
{
    public class PasswordValidatorTests
    {
        private readonly IPasswordValidator _validator;

        public PasswordValidatorTests()
        {
            _validator = new PasswordValidator();
        }

        [Theory]
        [InlineData("Password1!")] // Valid: has upper, lower, digit, special
        [InlineData("MyP@ssw0rd")] // Valid
        [InlineData("C0mpl3x!Pass")] // Valid
        public void Validate_ShouldReturnSuccess_WhenPasswordIsValid(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenPasswordIsTooShort()
        {
            // Arrange
            var password = "Pass1!"; // Only 6 characters

            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("at least 8 characters");
        }

        [Fact]
        public void Validate_ShouldReturnFailure_WhenPasswordIsTooLong()
        {
            // Arrange
            var password = new string('a', 101) + "A1!"; // 104 characters

            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("must not exceed 100 characters");
        }

        [Theory]
        [InlineData("password123!")] // No uppercase
        [InlineData("alllowercase1!")]
        public void Validate_ShouldReturnFailure_WhenPasswordHasNoUppercase(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("uppercase letter");
        }

        [Theory]
        [InlineData("PASSWORD123!")] // No lowercase
        [InlineData("ALLUPPERCASE1!")]
        public void Validate_ShouldReturnFailure_WhenPasswordHasNoLowercase(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("lowercase letter");
        }

        [Theory]
        [InlineData("Password!")] // No digit
        [InlineData("NoDigitsHere!")]
        public void Validate_ShouldReturnFailure_WhenPasswordHasNoDigit(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("digit");
        }

        [Theory]
        [InlineData("Password123")] // No special character
        [InlineData("NoSpecialChar1")]
        public void Validate_ShouldReturnFailure_WhenPasswordHasNoSpecialCharacter(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("special character");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldReturnFailure_WhenPasswordIsNullOrEmpty(string password)
        {
            // Act
            var result = _validator.Validate(password);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("required");
        }
    }
}
