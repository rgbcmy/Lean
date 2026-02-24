using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Stocks controller for stock search and information
/// 股票控制器，用于股票搜索和信息查询
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/stocks")]
[Authorize]
public class StocksController : ControllerBase
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<StocksController> _logger;

    public StocksController(
        IMarketDataService marketDataService,
        ILogger<StocksController> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    /// <summary>
    /// Search for stocks by symbol or company name
    /// 根据代码或公司名称搜索股票
    /// </summary>
    /// <param name="q">Search query (symbol or company name)</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 50)</param>
    /// <response code="200">Returns list of matching stocks</response>
    /// <response code="400">Invalid query parameter</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(StockSearchResult[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchStocks(
        [FromQuery] string? q,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Search query is required / 搜索查询不能为空"));
        }

        if (q.Length < 1)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Search query must be at least 1 character / 搜索查询至少需要1个字符"));
        }

        if (limit < 1 || limit > 50)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Limit must be between 1 and 50 / 返回数量必须在1到50之间"));
        }

        try
        {
            _logger.LogInformation("Searching stocks with query: {Query}, limit: {Limit}", q, limit);
            
            var results = await _marketDataService.SearchStocksAsync(q, limit);
            
            // Return array directly to match frontend expectation
            return Ok(results.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stocks with query: {Query}", q);
            return StatusCode(500, new ErrorResponse(
                "INTERNAL_ERROR",
                "Failed to search stocks / 搜索股票失败"));
        }
    }

    /// <summary>
    /// Get detailed information about a specific stock
    /// 获取特定股票的详细信息
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <response code="200">Returns stock information</response>
    /// <response code="404">Stock not found</response>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(StockInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockInfo(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Stock symbol is required / 股票代码不能为空"));
        }

        try
        {
            var stockInfo = await _marketDataService.GetStockInfoAsync(symbol.ToUpperInvariant());
            
            if (stockInfo == null)
            {
                return NotFound(new ErrorResponse(
                    "NOT_FOUND",
                    $"Stock not found: {symbol} / 未找到股票: {symbol}"));
            }

            return Ok(stockInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock info for symbol: {Symbol}", symbol);
            return StatusCode(500, new ErrorResponse(
                "INTERNAL_ERROR",
                "Failed to get stock information / 获取股票信息失败"));
        }
    }

    /// <summary>
    /// Get list of popular/trending stocks
    /// 获取热门股票列表
    /// </summary>
    /// <param name="limit">Maximum number of results (default: 20)</param>
    /// <response code="200">Returns list of popular stocks</response>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(StockSearchResult[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularStocks([FromQuery] int limit = 20)
    {
        try
        {
            var results = await _marketDataService.GetPopularStocksAsync(limit);
            return Ok(results.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular stocks");
            return StatusCode(500, new ErrorResponse(
                "INTERNAL_ERROR",
                "Failed to get popular stocks / 获取热门股票失败"));
        }
    }
}
