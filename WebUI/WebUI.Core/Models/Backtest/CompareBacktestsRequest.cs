using System;

namespace WebUI.Core.Models.Backtest
{
    /// <summary>
    /// Backtest comparison request / 回测对比请求
    /// </summary>
    public class CompareBacktestsRequest
    {
        /// <summary>
        /// Backtest IDs to compare (2-4 backtests) / 要对比的回测 ID（2-4 个）
        /// </summary>
        public int[] BacktestIds { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Backtest comparison response / 回测对比响应
    /// </summary>
    public class CompareBacktestsResponse
    {
        public BacktestComparisonItem[] Backtests { get; set; } = Array.Empty<BacktestComparisonItem>();
        public int? BestBacktestId { get; set; } // ID of best performing backtest
        public string BestBy { get; set; } = string.Empty; // Metric used for ranking (e.g., "SharpeRatio")
    }

    /// <summary>
    /// Single backtest item in comparison / 对比中的单个回测项
    /// </summary>
    public class BacktestComparisonItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StrategyName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InitialCapital { get; set; }
        public string? BenchmarkSymbol { get; set; }
        public bool IsBest { get; set; } // Marked as best performer

        // Performance metrics for comparison
        public BacktestPerformanceMetrics? Performance { get; set; }

        // Simplified equity curve for comparison chart
        public EquityPoint[]? EquityCurve { get; set; }
    }
}
