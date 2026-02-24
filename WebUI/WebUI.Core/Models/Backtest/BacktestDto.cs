using System;

namespace WebUI.Core.Models.Backtest
{
    /// <summary>
    /// Backtest data transfer object / 回测数据传输对象
    /// </summary>
    public class BacktestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StrategyId { get; set; }
        public string StrategyName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal InitialCapital { get; set; }
        public string? BenchmarkSymbol { get; set; }
        public string DataResolution { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? Progress { get; set; }
        public string? ErrorMessage { get; set; }

        // Results summary
        public decimal? TotalReturn { get; set; }
        public decimal? AnnualReturn { get; set; }
        public decimal? SharpeRatio { get; set; }
        public decimal? MaxDrawdown { get; set; }
        public decimal? WinRate { get; set; }
        public int? TotalTrades { get; set; }
    }

    /// <summary>
    /// Backtest list response / 回测列表响应
    /// </summary>
    public class BacktestListResponse
    {
        public int Total { get; set; }
        public BacktestDto[] Backtests { get; set; } = Array.Empty<BacktestDto>();
    }
}
