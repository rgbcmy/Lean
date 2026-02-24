using System.Threading.Tasks;
using WebUI.Core.Models.Backtest;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Backtest execution service interface / 回测执行服务接口
    /// </summary>
    public interface IBacktestExecutionService
    {
        /// <summary>
        /// Start backtest execution / 开始回测执行
        /// </summary>
        Task StartBacktestAsync(int backtestId, int userId);

        /// <summary>
        /// Stop backtest execution / 停止回测执行
        /// </summary>
        Task StopBacktestAsync(int backtestId, int userId);

        /// <summary>
        /// Get running backtest status / 获取正在运行的回测状态
        /// </summary>
        Task<BacktestStatusDto?> GetBacktestStatusAsync(int backtestId);

        /// <summary>
        /// Check if backtest is running / 检查回测是否正在运行
        /// </summary>
        bool IsBacktestRunning(int backtestId);
    }

    /// <summary>
    /// Backtest status DTO / 回测状态数据传输对象
    /// </summary>
    public class BacktestStatusDto
    {
        public int BacktestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? Progress { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
