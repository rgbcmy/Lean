using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Market data controller / 市场数据控制器
/// Provides real-time and historical market data
/// 提供实时和历史市场数据
/// </summary>
[ApiController]
[Route("api/v1/market")]
[Authorize]
public class MarketController : ControllerBase
{
    private readonly ILogger<MarketController> _logger;
    private readonly IMarketDataService _marketDataService;

    public MarketController(
        ILogger<MarketController> logger,
        IMarketDataService marketDataService)
    {
        _logger = logger;
        _marketDataService = marketDataService;
    }

    /// <summary>
    /// Subscribe to market data for symbols / 订阅股票行情
    /// </summary>
    /// <param name="request">Subscription request / 订阅请求</param>
    /// <response code="200">Subscription successful / 订阅成功</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] MarketSubscriptionRequest request)
    {
        if (request.Symbols == null || request.Symbols.Count == 0)
        {
            return BadRequest(new WebUI.Core.Models.ErrorResponse(new WebUI.Core.Models.ErrorDetail(
                "VALIDATION_ERROR",
                "At least one symbol is required / 至少需要一个股票代码")));
        }

        if (request.Symbols.Count > 50)
        {
            return BadRequest(new WebUI.Core.Models.ErrorResponse(new WebUI.Core.Models.ErrorDetail(
                "VALIDATION_ERROR",
                "Maximum 50 symbols allowed / 最多允许 50 个股票代码")));
        }

        var success = await _marketDataService.SubscribeAsync(
            request.Symbols,
            request.RealTime);

        if (success)
        {
            _logger.LogInformation("User subscribed to {Count} symbols", request.Symbols.Count);
            
            return Ok(new SubscriptionResponse
            {
                Success = true,
                Message = "Successfully subscribed / 订阅成功",
                SubscribedSymbols = request.Symbols,
                TotalSubscriptions = _marketDataService.SubscribedSymbols.Count
            });
        }

        return BadRequest(new WebUI.Core.Models.ErrorResponse(new WebUI.Core.Models.ErrorDetail(
            "INTERNAL_ERROR",
            "Failed to subscribe to market data / 订阅市场数据失败")));
    }

    /// <summary>
    /// Unsubscribe from market data / 取消订阅行情
    /// </summary>
    /// <param name="request">Unsubscribe request / 取消订阅请求</param>
    /// <response code="200">Unsubscription successful / 取消订阅成功</response>
    [HttpPost("unsubscribe")]
    [ProducesResponseType(typeof(SubscriptionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var success = await _marketDataService.UnsubscribeAsync(request.Symbols);

        return Ok(new SubscriptionResponse
        {
            Success = success,
            Message = success ? "Successfully unsubscribed / 取消订阅成功" : "Failed to unsubscribe / 取消订阅失败",
            SubscribedSymbols = request.Symbols,
            TotalSubscriptions = _marketDataService.SubscribedSymbols.Count
        });
    }

    /// <summary>
    /// Get current quote for a symbol / 获取股票当前行情
    /// </summary>
    /// <param name="symbol">Stock symbol / 股票代码</param>
    /// <response code="200">Quote data / 行情数据</response>
    /// <response code="404">Symbol not found / 股票代码不存在</response>
    [HttpGet("quote/{symbol}")]
    [ProducesResponseType(typeof(MarketQuote), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuote(string symbol)
    {
        var quote = await _marketDataService.GetQuoteAsync(symbol);

        if (quote == null)
        {
            return NotFound(new WebUI.Core.Models.ErrorResponse(new WebUI.Core.Models.ErrorDetail(
                "NOT_FOUND",
                $"Quote not found for symbol: {symbol} / 未找到股票行情：{symbol}",
                new { resource = symbol })));
        }

        return Ok(quote);
    }

    /// <summary>
    /// Get quotes for multiple symbols / 获取多个股票行情
    /// </summary>
    /// <param name="symbols">Comma-separated symbols / 逗号分隔的股票代码</param>
    /// <response code="200">List of quotes / 行情列表</response>
    [HttpGet("quotes")]
    [ProducesResponseType(typeof(List<MarketQuote>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuotes([FromQuery] string symbols)
    {
        var symbolList = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (symbolList.Count == 0)
        {
            return BadRequest(new WebUI.Core.Models.ErrorResponse(new WebUI.Core.Models.ErrorDetail(
                "VALIDATION_ERROR",
                "No symbols provided / 未提供股票代码")));
        }

        var quotes = await _marketDataService.GetQuotesAsync(symbolList);
        return Ok(quotes);
    }

    /// <summary>
    /// Get current market status / 获取当前市场状态
    /// </summary>
    /// <response code="200">Market status information / 市场状态信息</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(MarketStatusInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketStatus()
    {
        var status = await _marketDataService.GetMarketStatusAsync();
        return Ok(status);
    }

    /// <summary>
    /// Get market summary with indices, movers, and sectors / 获取市场概览
    /// </summary>
    /// <response code="200">Market summary / 市场概览</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(MarketSummary), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarketSummary()
    {
        var summary = await _marketDataService.GetMarketSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Get list of currently subscribed symbols / 获取当前订阅的股票列表
    /// </summary>
    /// <response code="200">List of subscribed symbols / 已订阅的股票列表</response>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(SubscriptionListResponse), StatusCodes.Status200OK)]
    public IActionResult GetSubscriptions()
    {
        return Ok(new SubscriptionListResponse
        {
            Symbols = _marketDataService.SubscribedSymbols.ToList(),
            Count = _marketDataService.SubscribedSymbols.Count,
            MaxSubscriptions = 50
        });
    }
}

/// <summary>
/// Subscription response / 订阅响应
/// </summary>
public class SubscriptionResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public required List<string> SubscribedSymbols { get; set; }
    public int TotalSubscriptions { get; set; }
}

/// <summary>
/// Unsubscribe request / 取消订阅请求
/// </summary>
public class UnsubscribeRequest
{
    public required List<string> Symbols { get; set; }
}

/// <summary>
/// Subscription list response / 订阅列表响应
/// </summary>
public class SubscriptionListResponse
{
    public required List<string> Symbols { get; set; }
    public int Count { get; set; }
    public int MaxSubscriptions { get; set; }
}
