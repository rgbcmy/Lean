using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Core.Models.Backtest
{
    /// <summary>
    /// Request to create a new backtest / 创建新回测的请求
    /// </summary>
    public class CreateBacktestRequest
    {
        /// <summary>
        /// Strategy ID to backtest / 回测的策略 ID
        /// </summary>
        [Required(ErrorMessage = "Strategy ID is required / 策略 ID 必填")]
        public int StrategyId { get; set; }

        /// <summary>
        /// Backtest name / 回测名称
        /// </summary>
        [Required(ErrorMessage = "Name is required / 名称必填")]
        [MaxLength(200, ErrorMessage = "Name too long / 名称过长")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Backtest description / 回测描述
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Description too long / 描述过长")]
        public string? Description { get; set; }

        /// <summary>
        /// Start date for backtest / 回测开始日期
        /// </summary>
        [Required(ErrorMessage = "Start date is required / 开始日期必填")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for backtest / 回测结束日期
        /// </summary>
        [Required(ErrorMessage = "End date is required / 结束日期必填")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Initial capital in USD / 初始资金（美元）
        /// </summary>
        [Required(ErrorMessage = "Initial capital is required / 初始资金必填")]
        [Range(1000, 100000000, ErrorMessage = "Initial capital must be between $1,000 and $100,000,000 / 初始资金必须在 1000-100000000 美元之间")]
        public decimal InitialCapital { get; set; } = 100000;

        /// <summary>
        /// Benchmark symbol (e.g., SPY) / 基准指数代码（如 SPY）
        /// </summary>
        [MaxLength(50)]
        public string? BenchmarkSymbol { get; set; }

        /// <summary>
        /// Data resolution: Daily, Hourly, Minute / 数据频率：日线、小时线、分钟线
        /// </summary>
        [Required(ErrorMessage = "Data resolution is required / 数据频率必填")]
        [RegularExpression("^(Daily|Hourly|Minute)$", ErrorMessage = "Invalid data resolution / 数据频率无效")]
        public string DataResolution { get; set; } = "Daily";

        /// <summary>
        /// Additional strategy parameters (JSON) / 额外的策略参数（JSON）
        /// </summary>
        public string? ParametersJson { get; set; }
    }
}
