using System;

namespace WebUI.Core.Security
{
    /// <summary>
    /// Interface for password hashing operations
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a password using PBKDF2 with 10,000 iterations
        /// </summary>
        /// <param name="password">Plain text password to hash</param>
        /// <returns>Hashed password in format: salt.hash</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="hashedPassword">Hashed password in format: salt.hash</param>
        /// <returns>True if password matches, false otherwise</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
