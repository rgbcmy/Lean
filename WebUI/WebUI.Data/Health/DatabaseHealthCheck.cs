using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Data.Health
{
    /// <summary>
    /// Database health check for monitoring
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly WebUIDbContext _context;

        public DatabaseHealthCheck(WebUIDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to connect and execute a simple query
                await _context.Database.CanConnectAsync(cancellationToken);
                
                // Check if migrations are applied
                var pendingMigrations = await _context.Database
                    .GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    return HealthCheckResult.Degraded(
                        $"Database has {pendingMigrations.Count()} pending migrations");
                }

                // Get database provider
                var provider = _context.Database.ProviderName ?? "Unknown";
                
                var data = new Dictionary<string, object>
                {
                    { "provider", provider },
                    { "connectionState", "Connected" }
                };

                return HealthCheckResult.Healthy("Database is healthy", data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Database is unhealthy",
                    ex);
            }
        }
    }
}
