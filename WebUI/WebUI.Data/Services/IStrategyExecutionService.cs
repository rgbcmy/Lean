using WebUI.Core.Models;

namespace WebUI.Data.Services;

/// <summary>
/// Strategy execution service interface / 策略执行服务接口
/// Handles strategy process lifecycle, monitoring, and scheduling
/// </summary>
public interface IStrategyExecutionService
{
    /// <summary>
    /// Start a strategy / 启动策略
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="request">Start request / 启动请求</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Execution detail / 执行详情</returns>
    Task<StrategyExecutionDetailDto> StartStrategyAsync(
        int strategyId, 
        int userId, 
        StartStrategyRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a running strategy / 停止运行中的策略
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="request">Stop request / 停止请求</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task StopStrategyAsync(
        int strategyId, 
        int userId, 
        StopStrategyRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy runtime status / 获取策略运行时状态
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Runtime status / 运行时状态</returns>
    Task<StrategyRuntimeStatusDto> GetStrategyRuntimeStatusAsync(
        int strategyId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy execution history / 获取策略执行历史
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="page">Page number / 页码</param>
    /// <param name="pageSize">Page size / 每页数量</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Execution list / 执行列表</returns>
    Task<StrategyExecutionListResponse> GetStrategyExecutionsAsync(
        int strategyId, 
        int userId, 
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get execution details / 获取执行详情
    /// </summary>
    /// <param name="executionId">Execution ID / 执行 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    /// <returns>Execution detail / 执行详情</returns>
    Task<StrategyExecutionDetailDto?> GetExecutionDetailsAsync(
        int executionId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule strategy execution / 定时执行策略
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="request">Schedule request / 调度请求</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task ScheduleStrategyAsync(
        int strategyId, 
        int userId, 
        ScheduleStrategyExecutionRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel scheduled execution / 取消定时执行
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <param name="userId">User ID / 用户 ID</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task CancelScheduleAsync(
        int strategyId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a strategy is currently running / 检查策略是否正在运行
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <returns>True if running / 正在运行返回 true</returns>
    Task<bool> IsStrategyRunningAsync(int strategyId);

    /// <summary>
    /// Get process resource usage / 获取进程资源使用情况
    /// </summary>
    /// <param name="strategyId">Strategy ID / 策略 ID</param>
    /// <returns>CPU and memory usage / CPU 和内存使用情况</returns>
    Task<(double cpuUsage, long memoryMB)?> GetProcessResourceUsageAsync(int strategyId);
}
