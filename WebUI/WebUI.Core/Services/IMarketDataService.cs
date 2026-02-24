using WebUI.Core.Models;

namespace WebUI.Core.Services;

/// <summary>
/// Market data service interface / 行情数据服务接口
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Subscribe to market data for symbols / 订阅股票行情
    /// </summary>
    /// <param name="symbols">Stock symbols / 股票代码列表</param>
    /// <param name="realTime">Real-time or delayed / 实时或延迟</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<bool> SubscribeAsync(IEnumerable<string> symbols, bool realTime = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe from market data / 取消订阅行情
    /// </summary>
    /// <param name="symbols">Stock symbols / 股票代码列表</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<bool> UnsubscribeAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current quote for a symbol / 获取股票当前行情
    /// </summary>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<MarketQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get quotes for multiple symbols / 获取多个股票行情
    /// </summary>
    /// <param name="symbols">Stock symbols / 股票代码列表</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<List<MarketQuote>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current market status / 获取当前市场状态
    /// </summary>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<MarketStatusInfo> GetMarketStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market summary with indices, movers, and sectors / 获取市场概览
    /// </summary>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<MarketSummary> GetMarketSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for stocks by symbol or company name / 根据代码或公司名称搜索股票
    /// </summary>
    /// <param name="query">Search query / 搜索查询</param>
    /// <param name="limit">Maximum number of results / 最大结果数量</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<List<StockSearchResult>> SearchStocksAsync(string query, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed stock information / 获取详细股票信息
    /// </summary>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<StockInfo?> GetStockInfoAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of popular/trending stocks / 获取热门股票列表
    /// </summary>
    /// <param name="limit">Maximum number of results / 最大结果数量</param>
    /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
    Task<List<StockSearchResult>> GetPopularStocksAsync(int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event fired when market data is updated / 行情数据更新事件
    /// </summary>
    event EventHandler<MarketDataUpdate>? OnMarketDataUpdate;

    /// <summary>
    /// Get list of currently subscribed symbols / 获取当前订阅的股票列表
    /// </summary>
    IReadOnlyList<string> SubscribedSymbols { get; }
}
