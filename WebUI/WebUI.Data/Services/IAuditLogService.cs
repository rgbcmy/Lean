using System;
using System.Threading.Tasks;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Interface for audit logging operations
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Log an audit event
        /// </summary>
        /// <param name="userId">User ID (null for anonymous operations)</param>
        /// <param name="action">Action performed (e.g., "Login", "Logout", "ChangePassword")</param>
        /// <param name="result">Result of the action (e.g., "Success", "Failed")</param>
        /// <param name="details">Additional details about the action</param>
        /// <param name="ipAddress">IP address of the client (optional)</param>
        /// <returns>Task</returns>
        Task LogAsync(int? userId, string action, string result, string? details = null, string? ipAddress = null);

        /// <summary>
        /// Log a security event
        /// </summary>
        /// <param name="userId">User ID (null for anonymous operations)</param>
        /// <param name="eventType">Type of security event</param>
        /// <param name="details">Details of the event</param>
        /// <param name="ipAddress">IP address of the client (optional)</param>
        /// <returns>Task</returns>
        Task LogSecurityEventAsync(int? userId, string eventType, string details, string? ipAddress = null);
    }
}
