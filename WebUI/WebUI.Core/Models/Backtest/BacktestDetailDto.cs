using System;
using System.Text.Json;

namespace WebUI.Core.Models.Backtest
{
    /// <summary>
    /// Detailed backtest information / 详细的回测信息
    /// </summary>
    public class BacktestDetailDto
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
        public JsonElement? Parameters { get; set; }

        // Performance metrics
        public BacktestPerformanceMetrics? Performance { get; set; }

        // Charts data
        public BacktestChartsData? Charts { get; set; }

        // Trades
        public BacktestTrade[]? Trades { get; set; }
    }

    /// <summary>
    /// Backtest performance metrics / 回测性能指标
    /// </summary>
    public class BacktestPerformanceMetrics
    {
        public decimal? TotalReturn { get; set; }
        public decimal? AnnualReturn { get; set; }
        public decimal? SharpeRatio { get; set; }
        public decimal? MaxDrawdown { get; set; }
        public decimal? WinRate { get; set; }
        public int? TotalTrades { get; set; }
        public int? WinningTrades { get; set; }
        public int? LosingTrades { get; set; }
        public decimal? AverageWin { get; set; }
        public decimal? AverageLoss { get; set; }
        public decimal? ProfitFactor { get; set; }
        public decimal? Alpha { get; set; }
        public decimal? Beta { get; set; }
        public decimal? AverageHoldingPeriod { get; set; }
    }

    /// <summary>
    /// Backtest charts data / 回测图表数据
    /// </summary>
    public class BacktestChartsData
    {
        public EquityPoint[]? EquityCurve { get; set; }
        public DrawdownPoint[]? DrawdownCurve { get; set; }
    }

    /// <summary>
    /// Equity curve point / 权益曲线点
    /// </summary>
    public class EquityPoint
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Drawdown curve point / 回撤曲线点
    /// </summary>
    public class DrawdownPoint
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
    }

    /// <summary>
    /// Backtest trade record / 回测交易记录
    /// </summary>
    public class BacktestTrade
    {
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty; // Long, Short
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercent { get; set; }
        public decimal Mae { get; set; } // Maximum Adverse Excursion
        public decimal Mfe { get; set; } // Maximum Favorable Excursion
    }
}
