using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Data.Services;

/// <summary>
/// Trading service implementation / 交易服务实现
/// Handles order submission, query, and management
/// 处理订单提交、查询和管理
/// </summary>
public class TradingService : ITradingService
{
    private readonly ILogger<TradingService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIbkrConnectionService _ibkrConnection;

    // Simulated stock database for search (in production, this would query a symbol database or API)
    private static readonly Dictionary<string, StockSearchResult> _stockDatabase = new()
    {
        ["AAPL"] = new() { Symbol = "AAPL", Name = "Apple Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["MSFT"] = new() { Symbol = "MSFT", Name = "Microsoft Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["GOOGL"] = new() { Symbol = "GOOGL", Name = "Alphabet Inc. Class A", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["AMZN"] = new() { Symbol = "AMZN", Name = "Amazon.com Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["TSLA"] = new() { Symbol = "TSLA", Name = "Tesla Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["META"] = new() { Symbol = "META", Name = "Meta Platforms Inc.", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["NVDA"] = new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Exchange = "NASDAQ", AssetType = "Stock", Currency = "USD" },
        ["JPM"] = new() { Symbol = "JPM", Name = "JPMorgan Chase & Co.", Exchange = "NYSE", AssetType = "Stock", Currency = "USD" },
        ["V"] = new() { Symbol = "V", Name = "Visa Inc.", Exchange = "NYSE", AssetType = "Stock", Currency = "USD" },
        ["SPY"] = new() { Symbol = "SPY", Name = "SPDR S&P 500 ETF Trust", Exchange = "NYSE", AssetType = "ETF", Currency = "USD" },
        ["QQQ"] = new() { Symbol = "QQQ", Name = "Invesco QQQ Trust", Exchange = "NASDAQ", AssetType = "ETF", Currency = "USD" },
        ["IWM"] = new() { Symbol = "IWM", Name = "iShares Russell 2000 ETF", Exchange = "NYSE", AssetType = "ETF", Currency = "USD" },
    };

    public TradingService(
        ILogger<TradingService> logger,
        IUnitOfWork unitOfWork,
        IIbkrConnectionService ibkrConnection)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _ibkrConnection = ibkrConnection;
    }

    /// <summary>
    /// Search for stocks by symbol or name / 按代码或名称搜索股票
    /// </summary>
    public Task<StockSearchResponse> SearchStocksAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(new StockSearchResponse { Results = new List<StockSearchResult>(), TotalCount = 0 });
        }

        var searchQuery = query.Trim().ToUpperInvariant();

        // Search by symbol or name
        var results = _stockDatabase.Values
            .Where(s => s.Symbol.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                       s.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .Take(maxResults)
            .ToList();

        _logger.LogInformation("Stock search for '{Query}' returned {Count} results", query, results.Count);

        return Task.FromResult(new StockSearchResponse
        {
            Results = results,
            TotalCount = results.Count
        });
    }

    /// <summary>
    /// Submit a market order / 提交市价单
    /// </summary>
    public async Task<OrderSubmissionResponse> SubmitMarketOrderAsync(
        int brokerAccountId,
        MarketOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting market order: {Side} {Quantity} {Symbol}",
            request.Side, request.Quantity, request.Symbol);

        // Validate connection
        if (_ibkrConnection.ConnectionState.Status != IbkrConnectionStatus.Connected)
        {
            return new OrderSubmissionResponse
            {
                Success = false,
                Message = "IBKR not connected / IBKR 未连接"
            };
        }

        // Validate broker account exists
        var brokerAccount = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);
        if (brokerAccount == null)
        {
            return new OrderSubmissionResponse
            {
                Success = false,
                Message = "Broker account not found / 券商账户不存在"
            };
        }

        // Check buying power for Buy orders
        if (request.Side == "Buy")
        {
            var accountSummary = await _ibkrConnection.GetAccountSummaryAsync();
            if (accountSummary != null && accountSummary.BuyingPower < 100) // Simplified check
            {
                return new OrderSubmissionResponse
                {
                    Success = false,
                    Message = $"Insufficient buying power / 购买力不足，当前可用：${accountSummary.BuyingPower:F2}"
                };
            }
        }

        // Create order entity
        var order = new Order
        {
            BrokerAccountId = brokerAccountId,
            BrokerOrderId = GenerateBrokerOrderId(),
            Symbol = request.Symbol.ToUpperInvariant(),
            OrderType = "Market",
            Side = request.Side,
            Quantity = request.Quantity,
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Submit order to IBKR via IPC to Lean engine
        // This will be implemented when the Lean-IBKR bridge is complete
        // For now, simulate order submission
        _logger.LogInformation("Market order submitted with ID: {OrderId}, BrokerOrderId: {BrokerOrderId}",
            order.Id, order.BrokerOrderId);

        return new OrderSubmissionResponse
        {
            Success = true,
            OrderId = order.Id,
            BrokerOrderId = order.BrokerOrderId,
            Message = "Order submitted successfully / 订单已提交"
        };
    }

    /// <summary>
    /// Submit a limit order / 提交限价单
    /// </summary>
    public async Task<OrderSubmissionResponse> SubmitLimitOrderAsync(
        int brokerAccountId,
        LimitOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting limit order: {Side} {Quantity} {Symbol} @ ${LimitPrice}",
            request.Side, request.Quantity, request.Symbol, request.LimitPrice);

        // Validate connection
        if (_ibkrConnection.ConnectionState.Status != IbkrConnectionStatus.Connected)
        {
            return new OrderSubmissionResponse
            {
                Success = false,
                Message = "IBKR not connected / IBKR 未连接"
            };
        }

        // Validate broker account exists
        var brokerAccount = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);
        if (brokerAccount == null)
        {
            return new OrderSubmissionResponse
            {
                Success = false,
                Message = "Broker account not found / 券商账户不存在"
            };
        }

        // Check buying power for Buy orders
        if (request.Side == "Buy")
        {
            var estimatedCost = request.LimitPrice * request.Quantity;
            var accountSummary = await _ibkrConnection.GetAccountSummaryAsync();
            if (accountSummary != null && accountSummary.BuyingPower < estimatedCost)
            {
                return new OrderSubmissionResponse
                {
                    Success = false,
                    Message = $"Insufficient buying power / 购买力不足，当前可用：${accountSummary.BuyingPower:F2}"
                };
            }
        }

        // Create order entity
        var order = new Order
        {
            BrokerAccountId = brokerAccountId,
            BrokerOrderId = GenerateBrokerOrderId(),
            Symbol = request.Symbol.ToUpperInvariant(),
            OrderType = "Limit",
            Side = request.Side,
            Quantity = request.Quantity,
            LimitPrice = request.LimitPrice,
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Submit order to IBKR via IPC to Lean engine
        _logger.LogInformation("Limit order submitted with ID: {OrderId}, BrokerOrderId: {BrokerOrderId}",
            order.Id, order.BrokerOrderId);

        return new OrderSubmissionResponse
        {
            Success = true,
            OrderId = order.Id,
            BrokerOrderId = order.BrokerOrderId,
            Message = "Limit order submitted successfully / 限价单已提交",
            EstimatedCost = request.LimitPrice * request.Quantity
        };
    }

    /// <summary>
    /// Cancel an order / 取消订单
    /// </summary>
    public async Task<OrderCancelResponse> CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to cancel order: {OrderId}", orderId);

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return new OrderCancelResponse
            {
                Success = false,
                OrderId = orderId,
                Message = "Order not found / 订单不存在"
            };
        }

        // Check if order can be cancelled
        if (order.Status == "Filled")
        {
            return new OrderCancelResponse
            {
                Success = false,
                OrderId = orderId,
                Message = "Order already filled, cannot cancel / 订单已完全成交，无法取消"
            };
        }

        if (order.Status == "Cancelled")
        {
            return new OrderCancelResponse
            {
                Success = false,
                OrderId = orderId,
                Message = "Order already cancelled / 订单已取消"
            };
        }

        if (order.Status == "Rejected")
        {
            return new OrderCancelResponse
            {
                Success = false,
                OrderId = orderId,
                Message = "Order was rejected, cannot cancel / 订单已被拒绝"
            };
        }

        // TODO: Send cancel request to IBKR via IPC
        // For now, update database status
        order.Status = "Cancelled";
        order.CancelledAt = DateTime.UtcNow;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order cancelled: {OrderId}", orderId);

        return new OrderCancelResponse
        {
            Success = true,
            OrderId = orderId,
            Message = order.FilledQuantity > 0
                ? $"Partial fill cancelled: {order.FilledQuantity}/{order.Quantity} filled / 部分成交订单已取消"
                : "Order cancelled successfully / 订单已取消"
        };
    }

    /// <summary>
    /// Get order by ID / 根据 ID 获取订单
    /// </summary>
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        return order == null ? null : MapToDto(order);
    }

    /// <summary>
    /// Query orders with filtering and pagination / 查询订单（支持筛选和分页）
    /// </summary>
    public async Task<OrderListResponse> QueryOrdersAsync(
        OrderQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying orders with filters: Symbol={Symbol}, Status={Status}, Page={Page}",
            parameters.Symbol, parameters.Status, parameters.PageNumber);

        // Build query
        var query = _unitOfWork.Orders.GetAllAsync(cancellationToken);
        var orders = (await query).AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.Symbol))
        {
            orders = orders.Where(o => o.Symbol == parameters.Symbol.ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            orders = orders.Where(o => o.Status == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Side))
        {
            orders = orders.Where(o => o.Side == parameters.Side);
        }

        if (!string.IsNullOrWhiteSpace(parameters.OrderType))
        {
            orders = orders.Where(o => o.OrderType == parameters.OrderType);
        }

        if (parameters.StartDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt <= parameters.EndDate.Value);
        }

        // Count total before pagination
        var totalCount = orders.Count();

        // Apply sorting
        orders = parameters.SortBy?.ToLowerInvariant() switch
        {
            "symbol" => parameters.SortDescending ? orders.OrderByDescending(o => o.Symbol) : orders.OrderBy(o => o.Symbol),
            "quantity" => parameters.SortDescending ? orders.OrderByDescending(o => o.Quantity) : orders.OrderBy(o => o.Quantity),
            "price" => parameters.SortDescending ? orders.OrderByDescending(o => o.LimitPrice) : orders.OrderBy(o => o.LimitPrice),
            _ => parameters.SortDescending ? orders.OrderByDescending(o => o.CreatedAt) : orders.OrderBy(o => o.CreatedAt)
        };

        // Apply pagination
        var pagedOrders = orders
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return new OrderListResponse
        {
            Orders = pagedOrders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    /// <summary>
    /// Estimate order cost / 预估订单成本
    /// </summary>
    public async Task<OrderCostEstimateResponse> EstimateOrderCostAsync(
        int brokerAccountId,
        OrderCostEstimateRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating order cost: {Side} {Quantity} {Symbol}",
            request.Side, request.Quantity, request.Symbol);

        // Get account summary for buying power check
        var accountSummary = await _ibkrConnection.GetAccountSummaryAsync();
        decimal availableBuyingPower = accountSummary?.BuyingPower ?? 0;

        // Estimate price (use limit price if provided, otherwise use a simulated market price)
        decimal estimatedPrice = request.LimitPrice ?? 100m; // In production, fetch real-time quote

        // Calculate costs
        decimal estimatedCost = estimatedPrice * request.Quantity;
        decimal estimatedCommission = CalculateCommission(request.Quantity);
        decimal totalCost = estimatedCost + estimatedCommission;

        bool sufficientFunds = request.Side == "Sell" || availableBuyingPower >= totalCost;

        string? warning = null;
        if (!sufficientFunds)
        {
            warning = $"Insufficient buying power. Required: ${totalCost:F2}, Available: ${availableBuyingPower:F2}";
        }

        return new OrderCostEstimateResponse
        {
            EstimatedCost = estimatedCost,
            EstimatedCommission = estimatedCommission,
            TotalCost = totalCost,
            AvailableBuyingPower = availableBuyingPower,
            SufficientFunds = sufficientFunds,
            Warning = warning
        };
    }

    // Helper methods

    private static string GenerateBrokerOrderId()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 50);
    }

    private static decimal CalculateCommission(int quantity)
    {
        // IBKR commission: $0.005 per share, minimum $1, maximum 1% of trade value
        // Simplified calculation
        decimal commission = quantity * 0.005m;
        return Math.Max(1m, commission);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BrokerOrderId = order.BrokerOrderId,
            Symbol = order.Symbol,
            OrderType = order.OrderType,
            Side = order.Side,
            Quantity = order.Quantity,
            FilledQuantity = order.FilledQuantity,
            LimitPrice = order.LimitPrice,
            FilledPrice = order.FilledPrice,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            FilledAt = order.FilledAt,
            CancelledAt = order.CancelledAt,
            ErrorMessage = order.ErrorMessage
        };
    }
}
