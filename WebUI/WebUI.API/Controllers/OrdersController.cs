using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Data.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Orders and trading controller / 订单和交易控制器
/// Provides stock trading, order management, and query APIs
/// 提供股票交易、订单管理和查询 API
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly ITradingService _tradingService;
    private const int DefaultBrokerAccountId = 1; // TODO: Get from user session/context

    public OrdersController(
        ILogger<OrdersController> logger,
        ITradingService tradingService)
    {
        _logger = logger;
        _tradingService = tradingService;
    }

    /// <summary>
    /// Search for stocks / 搜索股票
    /// </summary>
    /// <param name="q">Search query (symbol or company name) / 搜索关键词（股票代码或公司名称）</param>
    /// <param name="limit">Maximum results / 最大结果数</param>
    /// <response code="200">Search results / 搜索结果</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpGet("stocks/search")]
    [ProducesResponseType(typeof(StockSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchStocks(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Search query is required / 搜索关键词必填"));
        }

        if (limit < 1 || limit > 50)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Limit must be between 1 and 50 / 限制必须在 1 到 50 之间"));
        }

        var result = await _tradingService.SearchStocksAsync(q, limit);

        _logger.LogInformation("Stock search completed: query={Query}, results={Count}", q, result.TotalCount);

        return Ok(result);
    }

    /// <summary>
    /// Submit a market order / 提交市价单
    /// </summary>
    /// <param name="request">Market order request / 市价单请求</param>
    /// <response code="200">Order submitted / 订单已提交</response>
    /// <response code="400">Invalid request or insufficient funds / 请求无效或资金不足</response>
    [HttpPost("orders/market")]
    [ProducesResponseType(typeof(OrderSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitMarketOrder([FromBody] MarketOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid order request / 订单请求无效", errors));
        }

        var result = await _tradingService.SubmitMarketOrderAsync(DefaultBrokerAccountId, request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse("ORDER_SUBMISSION_FAILED", result.Message));
        }

        _logger.LogInformation("Market order submitted: {OrderId}, {Side} {Quantity} {Symbol}",
            result.OrderId, request.Side, request.Quantity, request.Symbol);

        return Ok(result);
    }

    /// <summary>
    /// Submit a limit order / 提交限价单
    /// </summary>
    /// <param name="request">Limit order request / 限价单请求</param>
    /// <response code="200">Order submitted / 订单已提交</response>
    /// <response code="400">Invalid request or insufficient funds / 请求无效或资金不足</response>
    [HttpPost("orders/limit")]
    [ProducesResponseType(typeof(OrderSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitLimitOrder([FromBody] LimitOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid order request / 订单请求无效", errors));
        }

        var result = await _tradingService.SubmitLimitOrderAsync(DefaultBrokerAccountId, request);

        if (!result.Success)
        {
            return BadRequest(new ErrorResponse("ORDER_SUBMISSION_FAILED", result.Message));
        }

        _logger.LogInformation("Limit order submitted: {OrderId}, {Side} {Quantity} {Symbol} @ ${LimitPrice}",
            result.OrderId, request.Side, request.Quantity, request.Symbol, request.LimitPrice);

        return Ok(result);
    }

    /// <summary>
    /// Cancel an order / 取消订单
    /// </summary>
    /// <param name="id">Order ID / 订单 ID</param>
    /// <response code="200">Order cancelled / 订单已取消</response>
    /// <response code="400">Cannot cancel order / 无法取消订单</response>
    /// <response code="404">Order not found / 订单不存在</response>
    [HttpDelete("orders/{id}")]
    [ProducesResponseType(typeof(OrderCancelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await _tradingService.CancelOrderAsync(id);

        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
            {
                return NotFound(new ErrorResponse("ORDER_NOT_FOUND", result.Message));
            }

            return BadRequest(new ErrorResponse("ORDER_CANCEL_FAILED", result.Message));
        }

        _logger.LogInformation("Order cancelled: {OrderId}", id);

        return Ok(result);
    }

    /// <summary>
    /// Query orders with filters / 查询订单（支持筛选）
    /// </summary>
    /// <param name="symbol">Filter by symbol / 按股票代码筛选</param>
    /// <param name="status">Filter by status / 按状态筛选</param>
    /// <param name="side">Filter by side (Buy/Sell) / 按方向筛选</param>
    /// <param name="orderType">Filter by order type / 按订单类型筛选</param>
    /// <param name="startDate">Start date filter / 开始日期筛选</param>
    /// <param name="endDate">End date filter / 结束日期筛选</param>
    /// <param name="pageNumber">Page number (default: 1) / 页码</param>
    /// <param name="pageSize">Page size (default: 50) / 每页数量</param>
    /// <param name="sortBy">Sort by field / 排序字段</param>
    /// <param name="sortDescending">Sort descending / 降序排序</param>
    /// <response code="200">Order list / 订单列表</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(OrderListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryOrders(
        [FromQuery] string? symbol = null,
        [FromQuery] string? status = null,
        [FromQuery] string? side = null,
        [FromQuery] string? orderType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        var parameters = new OrderQueryParameters
        {
            Symbol = symbol,
            Status = status,
            Side = side,
            OrderType = orderType,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _tradingService.QueryOrdersAsync(parameters);

        _logger.LogInformation("Order query completed: {Count} orders returned (page {Page})",
            result.Orders.Count, pageNumber);

        return Ok(result);
    }

    /// <summary>
    /// Get order details by ID / 根据 ID 获取订单详情
    /// </summary>
    /// <param name="id">Order ID / 订单 ID</param>
    /// <response code="200">Order details / 订单详情</response>
    /// <response code="404">Order not found / 订单不存在</response>
    [HttpGet("orders/{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _tradingService.GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound(new ErrorResponse(
                "ORDER_NOT_FOUND",
                $"Order with ID {id} not found / 订单 ID {id} 不存在"));
        }

        return Ok(order);
    }

    /// <summary>
    /// Estimate order cost / 预估订单成本
    /// </summary>
    /// <param name="request">Order cost estimate request / 订单成本预估请求</param>
    /// <response code="200">Cost estimate / 成本预估</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost("orders/estimate")]
    [ProducesResponseType(typeof(OrderCostEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EstimateOrderCost([FromBody] OrderCostEstimateRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid estimate request / 预估请求无效", errors));
        }

        var result = await _tradingService.EstimateOrderCostAsync(DefaultBrokerAccountId, request);

        _logger.LogInformation("Order cost estimated: {Symbol} {Quantity}, Total: ${TotalCost}",
            request.Symbol, request.Quantity, result.TotalCost);

        return Ok(result);
    }
}
