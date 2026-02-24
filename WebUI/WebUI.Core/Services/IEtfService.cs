using WebUI.Core.Models;

namespace WebUI.Core.Services;

/// <summary>
/// ETF service interface / ETF 服务接口
/// </summary>
public interface IEtfService
{
    /// <summary>
    /// Search ETFs / 搜索 ETF
    /// </summary>
    Task<EtfSearchResponse> SearchEtfsAsync(EtfSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get ETF details / 获取 ETF 详情
    /// </summary>
    Task<EtfDetail?> GetEtfDetailAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compare ETFs / 对比 ETF
    /// </summary>
    Task<EtfCompareResponse> CompareEtfsAsync(EtfCompareRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get ETF dividends / 获取 ETF 分红信息
    /// </summary>
    Task<EtfDividendResponse> GetEtfDividendsAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create recurring investment plan / 创建定投计划
    /// </summary>
    Task<RecurringPlanResponse> CreateRecurringPlanAsync(int userId, int brokerAccountId, RecurringPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recurring investment plans / 获取定投计划列表
    /// </summary>
    Task<List<RecurringPlanResponse>> GetRecurringPlansAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recurring investment plan by ID / 根据 ID 获取定投计划
    /// </summary>
    Task<RecurringPlanResponse?> GetRecurringPlanAsync(int userId, int planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update recurring investment plan / 更新定投计划
    /// </summary>
    Task<RecurringPlanResponse> UpdateRecurringPlanAsync(int userId, int planId, UpdateRecurringPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete recurring investment plan / 删除定投计划
    /// </summary>
    Task DeleteRecurringPlanAsync(int userId, int planId, CancellationToken cancellationToken = default);
}
