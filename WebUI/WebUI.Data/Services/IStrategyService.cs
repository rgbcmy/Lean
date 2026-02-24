using WebUI.Core.Models;

namespace WebUI.Data.Services;

/// <summary>
/// Strategy management service interface / 策略管理服务接口
/// </summary>
public interface IStrategyService
{
    /// <summary>
    /// Get all strategies for a user / 获取用户的所有策略
    /// </summary>
    Task<StrategyListResponse> GetStrategiesAsync(int userId, string? statusFilter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy by ID / 根据 ID 获取策略
    /// </summary>
    Task<StrategyDetailDto?> GetStrategyByIdAsync(int strategyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new strategy / 创建新策略
    /// </summary>
    Task<StrategyDetailDto> CreateStrategyAsync(int userId, CreateStrategyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update strategy / 更新策略
    /// </summary>
    Task<StrategyDetailDto?> UpdateStrategyAsync(int strategyId, int userId, UpdateStrategyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete strategy / 删除策略
    /// </summary>
    Task<bool> DeleteStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clone strategy / 克隆策略
    /// </summary>
    Task<StrategyDetailDto?> CloneStrategyAsync(int strategyId, int userId, CloneStrategyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy versions / 获取策略版本历史
    /// </summary>
    Task<List<StrategyVersionDto>> GetStrategyVersionsAsync(int strategyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback to a previous version / 回滚到历史版本
    /// </summary>
    Task<StrategyDetailDto?> RollbackToVersionAsync(int strategyId, int versionId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update strategy tags / 更新策略标签
    /// </summary>
    Task<StrategyDetailDto?> UpdateStrategyTagsAsync(int strategyId, int userId, string? tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strategy performance summary / 获取策略性能摘要
    /// </summary>
    Task<StrategyPerformanceDto?> GetStrategyPerformanceAsync(int strategyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload strategy code file / 上传策略代码文件
    /// </summary>
    Task<string> UploadStrategyCodeAsync(int strategyId, int userId, string fileName, Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export strategy as ZIP package / 导出策略为 ZIP 包
    /// </summary>
    Task<byte[]?> ExportStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import strategy from ZIP package / 从 ZIP 包导入策略
    /// </summary>
    Task<StrategyDetailDto?> ImportStrategyAsync(int userId, Stream zipStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate Lean configuration file / 生成 Lean 配置文件
    /// </summary>
    Task<string> GenerateLeanConfigAsync(int strategyId, int userId, CancellationToken cancellationToken = default);
}
