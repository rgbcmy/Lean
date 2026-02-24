namespace WebUI.Core.Services;

/// <summary>
/// Service for pushing market data updates via SignalR.
/// 通过 SignalR 推送行情数据更新的服务接口。
/// </summary>
public interface IMarketDataPushService
{
    /// <summary>
    /// Push market data update to all subscribers of a symbol.
    /// 向订阅指定股票的所有客户端推送行情数据更新。
    /// </summary>
    Task PushMarketDataAsync(string symbol, MarketDataUpdate update);

    /// <summary>
    /// Push market data updates for multiple symbols.
    /// 推送多个股票的行情数据更新。
    /// </summary>
    Task PushMarketDataBatchAsync(Dictionary<string, MarketDataUpdate> updates);
}

/// <summary>
/// Market data update model.
/// 行情数据更新模型。
/// </summary>
public class MarketDataUpdate
{
    /// <summary>
    /// Stock symbol (e.g., "AAPL")
    /// 股票代码
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Last trade price
    /// 最新成交价
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    /// Bid price
    /// 买一价
    /// </summary>
    public decimal BidPrice { get; set; }

    /// <summary>
    /// Ask price
    /// 卖一价
    /// </summary>
    public decimal AskPrice { get; set; }

    /// <summary>
    /// Trade volume
    /// 成交量
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Price change amount
    /// 涨跌额
    /// </summary>
    public decimal Change { get; set; }

    /// <summary>
    /// Price change percentage
    /// 涨跌幅
    /// </summary>
    public decimal ChangePercent { get; set; }

    /// <summary>
    /// Timestamp of the update (UTC)
    /// 更新时间戳 (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Market status (Open, Closed, PreMarket, AfterHours)
    /// 市场状态
    /// </summary>
    public string MarketStatus { get; set; } = string.Empty;
}
