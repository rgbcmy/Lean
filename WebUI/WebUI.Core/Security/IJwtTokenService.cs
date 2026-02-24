using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace WebUI.Core.Security
{
    /// <summary>
    /// JWT token generation and validation result
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// Access token (JWT)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Refresh token expiration time
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    /// <summary>
    /// Interface for JWT token generation and validation
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate access and refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username</param>
        /// <param name="additionalClaims">Additional claims to include in the token</param>
        /// <returns>Token result with access and refresh tokens</returns>
        TokenResult GenerateTokens(int userId, string username, Dictionary<string, string>? additionalClaims = null);

        /// <summary>
        /// Validate and decode a JWT access token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Claims principal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();
    }
}
