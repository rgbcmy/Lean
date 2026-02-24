using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models.Backtest
{
    /// <summary>
    /// Parameter optimization request / 参数优化请求
    /// </summary>
    public class OptimizeParametersRequest
    {
        /// <summary>
        /// Strategy ID to optimize / 要优化的策略 ID
        /// </summary>
        [Required]
        public int StrategyId { get; set; }

        /// <summary>
        /// Optimization name / 优化名称
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Start date for backtest / 回测开始日期
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for backtest / 回测结束日期
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Initial capital / 初始资金
        /// </summary>
        [Required]
        [Range(1000, 100000000)]
        public decimal InitialCapital { get; set; } = 100000;

        /// <summary>
        /// Parameter ranges to optimize / 要优化的参数范围
        /// </summary>
        [Required]
        [MinLength(1)]
        public ParameterRange[] Parameters { get; set; } = Array.Empty<ParameterRange>();

        /// <summary>
        /// Optimization metric (SharpeRatio, TotalReturn, MaxDrawdown, etc.) / 优化指标
        /// </summary>
        [Required]
        public string OptimizationMetric { get; set; } = "SharpeRatio";

        /// <summary>
        /// Maximum number of parallel backtests / 最大并行回测数
        /// </summary>
        [Range(1, 10)]
        public int MaxParallelBacktests { get; set; } = 3;
    }

    /// <summary>
    /// Parameter range for optimization / 参数优化范围
    /// </summary>
    public class ParameterRange
    {
        /// <summary>
        /// Parameter name / 参数名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Minimum value / 最小值
        /// </summary>
        [Required]
        public decimal Min { get; set; }

        /// <summary>
        /// Maximum value / 最大值
        /// </summary>
        [Required]
        public decimal Max { get; set; }

        /// <summary>
        /// Step size / 步长
        /// </summary>
        [Required]
        public decimal Step { get; set; }
    }

    /// <summary>
    /// Parameter optimization response / 参数优化响应
    /// </summary>
    public class OptimizeParametersResponse
    {
        public int OptimizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Pending, Running, Completed, Failed
        public int TotalCombinations { get; set; }
        public int CompletedCombinations { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Results (only available when completed)
        public OptimizationResult[]? Results { get; set; }
        public OptimizationResult? BestResult { get; set; }
    }

    /// <summary>
    /// Single optimization result / 单个优化结果
    /// </summary>
    public class OptimizationResult
    {
        public int BacktestId { get; set; }
        public string ParameterValues { get; set; } = string.Empty; // JSON representation
        public decimal? MetricValue { get; set; } // Value of the optimization metric
        public decimal? TotalReturn { get; set; }
        public decimal? SharpeRatio { get; set; }
        public decimal? MaxDrawdown { get; set; }
        public int? TotalTrades { get; set; }
        public bool IsBest { get; set; }
    }
}
