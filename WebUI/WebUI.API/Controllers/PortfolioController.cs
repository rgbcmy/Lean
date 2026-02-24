using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers
{
    /// <summary>
    /// Portfolio analytics and management controller / 投资组合分析和管理控制器
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioService portfolioService,
            ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService;
            _logger = logger;
        }

        /// <summary>
        /// Get portfolio allocation (asset distribution) / 获取投资组合配置（资产分布）
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Portfolio allocation / 投资组合配置</returns>
        /// <response code="200">Returns the portfolio allocation / 返回投资组合配置</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet("allocation")]
        [ProducesResponseType(typeof(PortfolioAllocationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioAllocationResponse>> GetAllocation(
            [FromQuery] int brokerAccountId = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting portfolio allocation for broker account {BrokerAccountId}", brokerAccountId);

                var allocation = await _portfolioService.GetPortfolioAllocationAsync(brokerAccountId, cancellationToken);
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio allocation for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to get portfolio allocation / 获取投资组合配置失败", ex.Message));
            }
        }

        /// <summary>
        /// Get equity curve data / 获取账户收益曲线数据
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="startDate">Start date (optional) / 开始日期（可选）</param>
        /// <param name="endDate">End date (optional) / 结束日期（可选）</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Equity curve data / 收益曲线数据</returns>
        /// <response code="200">Returns the equity curve / 返回收益曲线</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet("equity-curve")]
        [ProducesResponseType(typeof(EquityCurveResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EquityCurveResponse>> GetEquityCurve(
            [FromQuery] int brokerAccountId = 1,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting equity curve for broker account {BrokerAccountId}", brokerAccountId);

                var equityCurve = await _portfolioService.GetEquityCurveAsync(brokerAccountId, startDate, endDate, cancellationToken);
                return Ok(equityCurve);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equity curve for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to get equity curve / 获取收益曲线失败", ex.Message));
            }
        }

        /// <summary>
        /// Get portfolio summary / 获取投资组合摘要
        /// </summary>
        /// <param name="brokerAccountId">Broker account ID / 券商账户 ID</param>
        /// <param name="cancellationToken">Cancellation token / 取消令牌</param>
        /// <returns>Portfolio summary / 投资组合摘要</returns>
        /// <response code="200">Returns the portfolio summary / 返回投资组合摘要</response>
        /// <response code="401">Unauthorized / 未授权</response>
        /// <response code="500">Server error / 服务器错误</response>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(PortfolioPositionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioPositionsResponse>> GetSummary(
            [FromQuery] int brokerAccountId = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting portfolio summary for broker account {BrokerAccountId}", brokerAccountId);

                var positions = await _portfolioService.GetPositionsAsync(brokerAccountId, cancellationToken);
                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio summary for broker account {BrokerAccountId}", brokerAccountId);
                return StatusCode(500, ErrorResponse.ServerError("Failed to get portfolio summary / 获取投资组合摘要失败", ex.Message));
            }
        }
    }
}
