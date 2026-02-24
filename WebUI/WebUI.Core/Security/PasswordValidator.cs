using System;
using System.Text.RegularExpressions;

namespace WebUI.Core.Security
{
    /// <summary>
    /// Password validation result
    /// </summary>
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static PasswordValidationResult Success() => new PasswordValidationResult { IsValid = true };
        public static PasswordValidationResult Failure(string errorMessage) => 
            new PasswordValidationResult { IsValid = false, ErrorMessage = errorMessage };
    }

    /// <summary>
    /// Interface for password validation
    /// </summary>
    public interface IPasswordValidator
    {
        /// <summary>
        /// Validate password strength according to policy
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation result</returns>
        PasswordValidationResult Validate(string password);
    }

    /// <summary>
    /// Password validator implementing strong password policy
    /// </summary>
    public class PasswordValidator : IPasswordValidator
    {
        private const int MinLength = 8;
        private const int MaxLength = 100;

        /// <inheritdoc/>
        public PasswordValidationResult Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return PasswordValidationResult.Failure("Password is required");
            }

            if (password.Length < MinLength)
            {
                return PasswordValidationResult.Failure($"Password must be at least {MinLength} characters long");
            }

            if (password.Length > MaxLength)
            {
                return PasswordValidationResult.Failure($"Password must not exceed {MaxLength} characters");
            }

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return PasswordValidationResult.Failure("Password must contain at least one uppercase letter");
            }

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return PasswordValidationResult.Failure("Password must contain at least one lowercase letter");
            }

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return PasswordValidationResult.Failure("Password must contain at least one digit");
            }

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                return PasswordValidationResult.Failure("Password must contain at least one special character");
            }

            return PasswordValidationResult.Success();
        }
    }
}
