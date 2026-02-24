using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebUI.Data;
using WebUI.Data.Entities;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Audit log service for recording security and operational events
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly WebUIDbContext _dbContext;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            WebUIDbContext dbContext,
            ILogger<AuditLogService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task LogAsync(int? userId, string action, string result, string? details = null, string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityType = result, // Using EntityType for result (e.g., "Success", "Failed")
                    Details = details,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    Severity = DetermineSeverity(result)
                };

                _dbContext.AuditLogs.Add(auditLog);
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug("Audit log created: UserId={UserId}, Action={Action}, Result={Result}", 
                    userId, action, result);
            }
            catch (Exception ex)
            {
                // Don't fail the operation if audit logging fails
                _logger.LogError(ex, "Failed to create audit log: UserId={UserId}, Action={Action}", userId, action);
            }
        }

        private static string DetermineSeverity(string result)
        {
            if (result.Contains("Failed", StringComparison.OrdinalIgnoreCase) || 
                result.Contains("Error", StringComparison.OrdinalIgnoreCase))
            {
                return "Error";
            }
            if (result.Contains("Warning", StringComparison.OrdinalIgnoreCase))
            {
                return "Warning";
            }
            return "Info";
        }

        /// <inheritdoc/>
        public async Task LogSecurityEventAsync(int? userId, string eventType, string details, string? ipAddress = null)
        {
            await LogAsync(userId, $"Security:{eventType}", "Event", details, ipAddress);
            
            // Also log to the application logger for immediate visibility
            if (userId.HasValue)
            {
                _logger.LogWarning("Security event: {EventType} - UserId={UserId}, Details={Details}, IP={IpAddress}", 
                    eventType, userId, details, ipAddress);
            }
            else
            {
                _logger.LogWarning("Security event: {EventType} - Details={Details}, IP={IpAddress}", 
                    eventType, details, ipAddress);
            }
        }
    }
}
