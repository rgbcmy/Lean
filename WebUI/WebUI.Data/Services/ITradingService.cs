using WebUI.Core.Models;

namespace WebUI.Data.Services;

/// <summary>
/// Trading service interface / 交易服务接口
/// Handles order submission, query, and management
/// 处理订单提交、查询和管理
/// </summary>
public interface ITradingService
{
    /// <summary>
    /// Search for stocks by symbol or name / 按代码或名称搜索股票
    /// </summary>
    Task<StockSearchResponse> SearchStocksAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a market order / 提交市价单
    /// </summary>
    Task<OrderSubmissionResponse> SubmitMarketOrderAsync(int brokerAccountId, MarketOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a limit order / 提交限价单
    /// </summary>
    Task<OrderSubmissionResponse> SubmitLimitOrderAsync(int brokerAccountId, LimitOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an order / 取消订单
    /// </summary>
    Task<OrderCancelResponse> CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order by ID / 根据 ID 获取订单
    /// </summary>
    Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Query orders with filtering and pagination / 查询订单（支持筛选和分页）
    /// </summary>
    Task<OrderListResponse> QueryOrdersAsync(OrderQueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimate order cost / 预估订单成本
    /// </summary>
    Task<OrderCostEstimateResponse> EstimateOrderCostAsync(int brokerAccountId, OrderCostEstimateRequest request, CancellationToken cancellationToken = default);
}
