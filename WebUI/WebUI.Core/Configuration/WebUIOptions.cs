namespace WebUI.Core.Configuration
{
    /// <summary>
    /// WebUI configuration options
    /// </summary>
    public class WebUIOptions
    {
        public const string SectionName = "WebUI";

        public string DatabaseProvider { get; set; } = "SQLite";
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = "LeanWebUI";
        public string JwtAudience { get; set; } = "LeanWebUIClient";
        public int JwtExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
    }

    /// <summary>
    /// IBKR connection configuration
    /// </summary>
    public class IBKROptions
    {
        public const string SectionName = "IBKR";

        public string TWSHost { get; set; } = "127.0.0.1";
        public int TWSPort { get; set; } = 4002;
        public int PaperTradingPort { get; set; } = 4002;
        public int LiveTradingPort { get; set; } = 4001;
        public int ClientId { get; set; } = 1;
        public int ReconnectIntervalSeconds { get; set; } = 10;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// SignalR configuration
    /// </summary>
    public class SignalROptions
    {
        public const string SectionName = "SignalR";

        public int KeepAliveIntervalSeconds { get; set; } = 15;
        public int ClientTimeoutSeconds { get; set; } = 30;
        public int HandshakeTimeoutSeconds { get; set; } = 15;
    }

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        public int RequestsPerMinute { get; set; } = 60;
        public int BurstSize { get; set; } = 10;
    }

    /// <summary>
    /// CORS configuration
    /// </summary>
    public class CORSOptions
    {
        public const string SectionName = "CORS";

        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
