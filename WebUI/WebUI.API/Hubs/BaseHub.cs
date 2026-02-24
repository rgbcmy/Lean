using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebUI.API.Hubs;

/// <summary>
/// Base hub class with common functionality for all SignalR hubs.
/// 所有 SignalR Hub 的基类，提供通用功能。
/// </summary>
[Authorize]
public abstract class BaseHub : Hub
{
    protected readonly ILogger _logger;

    protected BaseHub(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user ID from the JWT claims.
    /// 从 JWT Claims 中获取当前用户 ID。
    /// </summary>
    protected string GetUserId()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims for connection {ConnectionId}", Context.ConnectionId);
            return string.Empty;
        }
        return userId;
    }

    /// <summary>
    /// Gets the current username from the JWT claims.
    /// 从 JWT Claims 中获取当前用户名。
    /// </summary>
    protected string GetUsername()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var username = GetUsername();
        
        _logger.LogInformation(
            "Client connected to {HubName}: ConnectionId={ConnectionId}, UserId={UserId}, Username={Username}",
            GetType().Name,
            Context.ConnectionId,
            userId,
            username
        );

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var reason = exception != null ? $"Error: {exception.Message}" : "Normal disconnect";
        
        _logger.LogInformation(
            "Client disconnected from {HubName}: ConnectionId={ConnectionId}, UserId={UserId}, Reason={Reason}",
            GetType().Name,
            Context.ConnectionId,
            userId,
            reason
        );

        if (exception != null)
        {
            _logger.LogError(exception, "Error during disconnection from {HubName}", GetType().Name);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
