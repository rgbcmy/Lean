using System.Threading.Tasks;
using WebUI.Core.Models.Backtest;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Backtest service interface / 回测服务接口
    /// </summary>
    public interface IBacktestService
    {
        /// <summary>
        /// Create a new backtest / 创建新回测
        /// </summary>
        Task<BacktestDetailDto> CreateBacktestAsync(CreateBacktestRequest request, int userId);

        /// <summary>
        /// Get all backtests for a user / 获取用户的所有回测
        /// </summary>
        Task<BacktestListResponse> GetBacktestsAsync(int userId, int? strategyId = null, string? status = null);

        /// <summary>
        /// Get backtest by ID / 根据 ID 获取回测
        /// </summary>
        Task<BacktestDetailDto?> GetBacktestByIdAsync(int id, int userId);

        /// <summary>
        /// Delete backtest / 删除回测
        /// </summary>
        Task<bool> DeleteBacktestAsync(int id, int userId);

        /// <summary>
        /// Update backtest status / 更新回测状态
        /// </summary>
        Task UpdateBacktestStatusAsync(int id, string status, int? progress = null, string? errorMessage = null);

        /// <summary>
        /// Update backtest results / 更新回测结果
        /// </summary>
        Task UpdateBacktestResultsAsync(int id, BacktestPerformanceMetrics metrics, BacktestChartsData charts, BacktestTrade[] trades);

        /// <summary>
        /// Compare multiple backtests / 对比多个回测
        /// </summary>
        Task<CompareBacktestsResponse> CompareBacktestsAsync(int[] backtestIds, int userId);
    }
}
