using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers
{
    /// <summary>
    /// Portfolio positions management controller / 投资组合持仓管理控制器
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class PositionsController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<PositionsController> _logger;

        public PositionsController(
            IPortfolioService portfolioService,
            ILogger<PositionsController> logger)
        {
            _portfolioService = portfolioService;
            _logger = logger;
        }

        /// <summary>
        /// Get all current positions / 获取所有当前持仓
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="symbolFilter">Filter by symbol (optional) / 按股票代码筛选（可选）</param>
        /// <param name="onlyProfitable">Show only profitable positions (optional) / 仅显示盈利持仓（可选）</param>
        /// <param name="onlyLosing">Show only losing positions (optional) / 仅显示亏损持仓（可选）</param>
        /// <param name="sortBy">Sort field (optional: Symbol, Quantity, MarketValue, UnrealizedPnL) / 排序字段</param>
        /// <param name="sortDirection">Sort direction (asc/desc) / 排序方向</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Portfolio positions / 投资组合持仓</returns>
        /// <response code="200">Returns the list of positions / 返回持仓列表</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet]
        [ProducesResponseType(typeof(PortfolioPositionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioPositionsResponse>> GetPositions(
            [FromQuery] int brokerAccountId = 1,
            [FromQuery] string? symbolFilter = null,
            [FromQuery] bool? onlyProfitable = null,
            [FromQuery] bool? onlyLosing = null,
            [FromQuery] string sortBy = "Symbol",
            [FromQuery] string sortDirection = "asc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting positions for broker account {BrokerAccountId}", brokerAccountId);

                PortfolioPositionsResponse response;

                if (symbolFilter != null || onlyProfitable != null || onlyLosing != null || 
                    sortBy != "Symbol" || sortDirection != "asc")
                {
                    var options = new PositionQueryOptions
                    {
                        SymbolFilter = symbolFilter,
                        OnlyProfitable = onlyProfitable,
                        OnlyLosing = onlyLosing,
                        SortBy = sortBy,
                        SortDirection = sortDirection
                    };
                    response = await _portfolioService.GetFilteredPositionsAsync(brokerAccountId, options, cancellationToken);
                }
                else
                {
                    response = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to get positions / 获取持仓失败", ex.Message));
            }
        }

        /// <summary>
        /// Get position detail by symbol / 获取指定股票的持仓详情
        /// </summary>
        /// <param name="symbol">Stock symbol / 股票代码</param>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Position detail / 持仓详情</returns>
        /// <response code="200">Returns the position detail / 返回持仓详情</response>
        /// <response code="404">Position not found / 未找到持仓</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet("{symbol}")]
        [ProducesResponseType(typeof(PositionDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PositionDetailResponse>> GetPositionDetail(
            string symbol,
            [FromQuery] int brokerAccountId = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting position detail for symbol {Symbol}", symbol);

                var position = await _portfolioService.GetPositionDetailAsync(brokerAccountId, symbol.ToUpper(), cancellationToken);
                if (position == null)
                {
                    return NotFound(ErrorResponse.NotFound(symbol, $"Position not found / 未找到持仓: {symbol}"));
                }

                return Ok(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting position detail for symbol {Symbol}", symbol);
                return StatusCode(500, ErrorResponse.ServerError("Failed to get position detail / 获取持仓详情失败", ex.Message));
            }
        }

        /// <summary>
        /// Close a position (sell entire or partial) / 平仓（全部或部分卖出）
        /// </summary>
        /// <param name="symbol">Stock symbol / 股票代码</param>
        /// <param name="request">Close position request / 平仓请求</param>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Order response / 订单响应</returns>
        /// <response code="200">Order successfully submitted / 订单成功提交</response>
        /// <response code="400">Invalid request / 无效请求</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpPost("{symbol}/close")]
        [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderResponse>> ClosePosition(
            string symbol,
            [FromBody] ClosePositionRequest request,
            [FromQuery] int brokerAccountId = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Closing position {Symbol} for broker account {BrokerAccountId}", symbol, brokerAccountId);

                // Validate request
                if (request.OrderType == "Limit" && request.LimitPrice == null)
                {
                    return BadRequest(ErrorResponse.ValidationError("Limit price required for limit orders / 限价单必须指定限价价格"));
                }

                var order = await _portfolioService.ClosePositionAsync(brokerAccountId, symbol.ToUpper(), request, cancellationToken);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when closing position {Symbol}", symbol);
                return BadRequest(ErrorResponse.ValidationError("Invalid operation / 无效操作", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing position {Symbol}", symbol);
                return StatusCode(500, ErrorResponse.ServerError("Failed to close position / 平仓失败", ex.Message));
            }
        }

        /// <summary>
        /// Export positions to CSV / 导出持仓为 CSV
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>CSV file / CSV 文件</returns>
        /// <response code="200">Returns CSV file / 返回 CSV 文件</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportPositions(
            [FromQuery] int brokerAccountId = 1,
            [FromQuery] string format = "csv",
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Exporting positions for broker account {BrokerAccountId} in format {Format}", brokerAccountId, format);

                byte[] fileContents;
                string contentType;
                string fileName;

                if (format.ToLower() == "excel")
                {
                    fileContents = await _portfolioService.ExportPositionsToExcelAsync(brokerAccountId, cancellationToken);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"positions_{DateTime.UtcNow:yyyyMMdd}.xlsx";
                }
                else
                {
                    fileContents = await _portfolioService.ExportPositionsToCsvAsync(brokerAccountId, cancellationToken);
                    contentType = "text/csv";
                    fileName = $"positions_{DateTime.UtcNow:yyyyMMdd}.csv";
                }

                return File(fileContents, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting positions for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to export positions / 导出持仓失败", ex.Message));
            }
        }

        /// <summary>
        /// Refresh position prices from market data / 从市场数据刷新持仓价格
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Success confirmation / 成功确认</returns>
        /// <response code="200">Prices updated successfully / 价格更新成功</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpPost("refresh-prices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshPrices(
            [FromQuery] int brokerAccountId = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Refreshing position prices for broker account {BrokerAccountId}", brokerAccountId);

                await _portfolioService.UpdatePositionPricesAsync(brokerAccountId, cancellationToken);
                return Ok(new { message = "Prices updated successfully / 价格更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing prices for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to refresh prices / 刷新价格失败", ex.Message));
            }
        }
    }
}
