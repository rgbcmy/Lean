using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace WebUI.Core.Middleware
{
    /// <summary>
    /// Middleware to add security headers to HTTP responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _options = new SecurityHeadersOptions();
            configuration.GetSection("Security").Bind(_options);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // X-Content-Type-Options: Prevent MIME sniffing
            if (_options.EnableXContentTypeOptions)
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            }

            // X-Frame-Options: Prevent clickjacking
            if (_options.EnableXFrameOptions)
            {
                context.Response.Headers["X-Frame-Options"] = _options.XFrameOptionsValue ?? "DENY";
            }

            // X-XSS-Protection: Enable browser XSS filter
            if (_options.EnableXXSSProtection)
            {
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Content-Security-Policy: Prevent XSS and injection attacks
            if (_options.EnableCSP && !string.IsNullOrWhiteSpace(_options.CSPPolicy))
            {
                context.Response.Headers["Content-Security-Policy"] = _options.CSPPolicy;
            }

            // Referrer-Policy: Control referrer information
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Permissions-Policy: Control browser features
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            await _next(context);
        }
    }

    public class SecurityHeadersOptions
    {
        public bool EnableCSP { get; set; } = true;
        public string? CSPPolicy { get; set; }
        public bool EnableXContentTypeOptions { get; set; } = true;
        public bool EnableXFrameOptions { get; set; } = true;
        public string? XFrameOptionsValue { get; set; } = "DENY";
        public bool EnableXXSSProtection { get; set; } = true;
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        /// <summary>
        /// Adds security headers middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
