namespace WebUI.Core.Configuration
{
    /// <summary>
    /// JWT authentication configuration settings
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key for signing JWT tokens (minimum 32 characters)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// JWT token issuer
        /// </summary>
        public string Issuer { get; set; } = "WebUI.API";

        /// <summary>
        /// JWT token audience
        /// </summary>
        public string Audience { get; set; } = "WebUI.Client";

        /// <summary>
        /// Access token expiration time in minutes (default: 60 minutes)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Refresh token expiration time in days (default: 7 days)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Validate the configuration
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is required");
            }

            if (SecretKey.Length < 32)
            {
                throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");
            }

            if (AccessTokenExpirationMinutes <= 0)
            {
                throw new InvalidOperationException("AccessTokenExpirationMinutes must be greater than 0");
            }

            if (RefreshTokenExpirationDays <= 0)
            {
                throw new InvalidOperationException("RefreshTokenExpirationDays must be greater than 0");
            }
        }
    }
}
