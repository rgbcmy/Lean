using System;

namespace WebUI.Core.Models;

/// <summary>
/// Market quote data (Level 1)
/// 市场行情数据（Level 1）
/// </summary>
public class MarketQuote
{
    /// <summary>
    /// Stock symbol / 股票代码
    /// </summary>
    public required string Symbol { get; set; }

    /// <summary>
    /// Last traded price / 最新成交价
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    /// Last trade size / 最新成交量
    /// </summary>
    public long LastSize { get; set; }

    /// <summary>
    /// Best bid price / 最佳买价
    /// </summary>
    public decimal BidPrice { get; set; }

    /// <summary>
    /// Bid size / 买盘数量
    /// </summary>
    public long BidSize { get; set; }

    /// <summary>
    /// Best ask price / 最佳卖价
    /// </summary>
    public decimal AskPrice { get; set; }

    /// <summary>
    /// Ask size / 卖盘数量
    /// </summary>
    public long AskSize { get; set; }

    /// <summary>
    /// Daily volume / 当日成交量
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Previous close price / 昨收价
    /// </summary>
    public decimal PreviousClose { get; set; }

    /// <summary>
    /// Price change from previous close / 涨跌额
    /// </summary>
    public decimal Change => LastPrice - PreviousClose;

    /// <summary>
    /// Price change percentage / 涨跌幅
    /// </summary>
    public decimal ChangePercent => PreviousClose != 0 ? (Change / PreviousClose) * 100 : 0;

    /// <summary>
    /// Today's high / 今日最高价
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Today's low / 今日最低价
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Today's open / 今日开盘价
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// Last update timestamp / 最后更新时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Is delayed quote (15-minute delay) / 是否为延迟行情（15 分钟延迟）
    /// </summary>
    public bool IsDelayed { get; set; }
}

/// <summary>
/// Market subscription request / 行情订阅请求
/// </summary>
public class MarketSubscriptionRequest
{
    /// <summary>
    /// Symbols to subscribe / 要订阅的股票代码列表
    /// </summary>
    public required List<string> Symbols { get; set; }

    /// <summary>
    /// Whether to subscribe to real-time data (requires IBKR subscription) / 是否订阅实时数据
    /// </summary>
    public bool RealTime { get; set; } = true;
}

/// <summary>
/// Market status / 市场状态
/// </summary>
public enum MarketStatus
{
    /// <summary>
    /// Market is closed / 市场休市
    /// </summary>
    Closed,

    /// <summary>
    /// Pre-market trading / 盘前交易
    /// </summary>
    PreMarket,

    /// <summary>
    /// Market is open / 市场开盘
    /// </summary>
    Open,

    /// <summary>
    /// After-hours trading / 盘后交易
    /// </summary>
    AfterHours
}

/// <summary>
/// Market status information / 市场状态信息
/// </summary>
public class MarketStatusInfo
{
    /// <summary>
    /// Current market status / 当前市场状态
    /// </summary>
    public MarketStatus Status { get; set; }

    /// <summary>
    /// Market timestamp (EST) / 市场时间（美东时间）
    /// </summary>
    public DateTime MarketTime { get; set; }

    /// <summary>
    /// Next market open time / 下次开盘时间
    /// </summary>
    public DateTime? NextOpen { get; set; }

    /// <summary>
    /// Next market close time / 下次闭市时间
    /// </summary>
    public DateTime? NextClose { get; set; }

    /// <summary>
    /// Is trading day / 是否为交易日
    /// </summary>
    public bool IsTradingDay { get; set; }
}

/// <summary>
/// Market summary / 市场概览
/// </summary>
public class MarketSummary
{
    /// <summary>
    /// Major indices / 主要指数
    /// </summary>
    public required List<IndexQuote> Indices { get; set; }

    /// <summary>
    /// Top gainers / 涨幅前10
    /// </summary>
    public required List<MarketQuote> TopGainers { get; set; }

    /// <summary>
    /// Top losers / 跌幅前10
    /// </summary>
    public required List<MarketQuote> TopLosers { get; set; }

    /// <summary>
    /// Sector performance / 行业表现
    /// </summary>
    public required List<SectorPerformance> Sectors { get; set; }

    /// <summary>
    /// Market status / 市场状态
    /// </summary>
    public required MarketStatusInfo MarketStatus { get; set; }
}

/// <summary>
/// Index quote / 指数行情
/// </summary>
public class IndexQuote
{
    /// <summary>
    /// Index symbol / 指数代码
    /// </summary>
    public required string Symbol { get; set; }

    /// <summary>
    /// Index name / 指数名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Current value / 当前值
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Change from previous close / 涨跌额
    /// </summary>
    public decimal Change { get; set; }

    /// <summary>
    /// Change percentage / 涨跌幅
    /// </summary>
    public decimal ChangePercent { get; set; }
}

/// <summary>
/// Sector performance / 行业表现
/// </summary>
public class SectorPerformance
{
    /// <summary>
    /// Sector name / 行业名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Sector symbol / 行业代码
    /// </summary>
    public required string Symbol { get; set; }

    /// <summary>
    /// Change percentage / 涨跌幅
    /// </summary>
    public decimal ChangePercent { get; set; }
}

/// <summary>
/// Market data update event / 行情数据更新事件
/// </summary>
public class MarketDataUpdate
{
    /// <summary>
    /// Update type / 更新类型
    /// </summary>
    public required string UpdateType { get; set; }

    /// <summary>
    /// Quote data / 行情数据
    /// </summary>
    public required MarketQuote Quote { get; set; }

    /// <summary>
    /// Update timestamp / 更新时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
