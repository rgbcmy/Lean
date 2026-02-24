using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// ETF trading controller / ETF 交易控制器
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/etfs")]
public class EtfsController : ControllerBase
{
    private readonly IEtfService _etfService;
    private readonly ILogger<EtfsController> _logger;

    public EtfsController(IEtfService etfService, ILogger<EtfsController> logger)
    {
        _etfService = etfService;
        _logger = logger;
    }

    /// <summary>
    /// Search for ETFs / 搜索 ETF
    /// </summary>
    /// <param name="request">Search parameters / 搜索参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ETF search results / ETF 搜索结果</returns>
    /// <response code="200">Returns search results / 返回搜索结果</response>
    /// <response code="400">Invalid request / 无效请求</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(EtfSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchEtfs([FromQuery] EtfSearchRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ETF search request: Query={Query}, Category={Category}, Page={Page}",
            request.Query, request.Category, request.Page);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ErrorResponse.ValidationError("Invalid search parameters / 无效的搜索参数", errors));
        }

        var response = await _etfService.SearchEtfsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get ETF details / 获取 ETF 详情
    /// </summary>
    /// <param name="symbol">ETF symbol / ETF 代码</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ETF details / ETF 详细信息</returns>
    /// <response code="200">Returns ETF details / 返回 ETF 详情</response>
    /// <response code="404">ETF not found / ETF 未找到</response>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(EtfDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEtfDetail(string symbol, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting ETF details for {Symbol}", symbol);

        var detail = await _etfService.GetEtfDetailAsync(symbol, cancellationToken);
        if (detail == null)
        {
            return NotFound(ErrorResponse.NotFound(symbol, $"ETF not found: {symbol} / ETF 未找到: {symbol}"));
        }

        return Ok(detail);
    }

    /// <summary>
    /// Compare ETFs / 对比 ETF
    /// </summary>
    /// <param name="request">Comparison request / 对比请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ETF comparison / ETF 对比结果</returns>
    /// <response code="200">Returns comparison results / 返回对比结果</response>
    /// <response code="400">Invalid request / 无效请求</response>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(EtfCompareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompareEtfs([FromBody] EtfCompareRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Comparing ETFs: {Symbols}", string.Join(", ", request.Symbols));

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ErrorResponse.ValidationError("Invalid comparison request / 无效的对比请求", errors));
        }

        var response = await _etfService.CompareEtfsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get ETF dividend information / 获取 ETF 分红信息
    /// </summary>
    /// <param name="symbol">ETF symbol / ETF 代码</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Dividend information / 分红信息</returns>
    /// <response code="200">Returns dividend information / 返回分红信息</response>
    /// <response code="404">ETF not found / ETF 未找到</response>
    [HttpGet("{symbol}/dividends")]
    [ProducesResponseType(typeof(EtfDividendResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEtfDividends(string symbol, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting dividend information for {Symbol}", symbol);

        try
        {
            var response = await _etfService.GetEtfDividendsAsync(symbol, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dividend information for {Symbol}", symbol);
            return NotFound(new ErrorResponse("DIVIDEND_ERROR", $"Failed to get dividend information for {symbol} / 获取 {symbol} 分红信息失败"));
        }
    }

    #region Recurring Investment Plans

    /// <summary>
    /// Create recurring investment plan / 创建定投计划
    /// </summary>
    /// <param name="request">Plan request / 计划请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Created plan / 创建的计划</returns>
    /// <response code="201">Plan created / 计划已创建</response>
    /// <response code="400">Invalid request / 无效请求</response>
    [HttpPost("recurring-plans")]
    [ProducesResponseType(typeof(RecurringPlanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRecurringPlan([FromBody] RecurringPlanRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ErrorResponse.ValidationError("Invalid recurring plan request / 无效的定投计划请求", errors));
        }

        // TODO: Get userId and brokerAccountId from authenticated user context
        const int userId = 1;
        const int brokerAccountId = 1;

        var response = await _etfService.CreateRecurringPlanAsync(userId, brokerAccountId, request, cancellationToken);
        
        return CreatedAtAction(nameof(GetRecurringPlan), new { planId = response.Id }, response);
    }

    /// <summary>
    /// Get all recurring investment plans / 获取所有定投计划
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of plans / 计划列表</returns>
    /// <response code="200">Returns plans / 返回计划列表</response>
    [HttpGet("recurring-plans")]
    [ProducesResponseType(typeof(List<RecurringPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecurringPlans(CancellationToken cancellationToken)
    {
        // TODO: Get userId from authenticated user context
        const int userId = 1;

        var plans = await _etfService.GetRecurringPlansAsync(userId, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// Get recurring investment plan by ID / 根据 ID 获取定投计划
    /// </summary>
    /// <param name="planId">Plan ID / 计划 ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Plan details / 计划详情</returns>
    /// <response code="200">Returns plan / 返回计划</response>
    /// <response code="404">Plan not found / 计划未找到</response>
    [HttpGet("recurring-plans/{planId}")]
    [ProducesResponseType(typeof(RecurringPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecurringPlan(int planId, CancellationToken cancellationToken)
    {
        // TODO: Get userId from authenticated user context
        const int userId = 1;

        var plan = await _etfService.GetRecurringPlanAsync(userId, planId, cancellationToken);
        if (plan == null)
        {
            return NotFound(ErrorResponse.NotFound("plan", $"Recurring plan {planId} not found / 定投计划 {planId} 未找到"));
        }

        return Ok(plan);
    }

    /// <summary>
    /// Update recurring investment plan / 更新定投计划
    /// </summary>
    /// <param name="planId">Plan ID / 计划 ID</param>
    /// <param name="request">Update request / 更新请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Updated plan / 更新的计划</returns>
    /// <response code="200">Plan updated / 计划已更新</response>
    /// <response code="400">Invalid request / 无效请求</response>
    /// <response code="404">Plan not found / 计划未找到</response>
    [HttpPut("recurring-plans/{planId}")]
    [ProducesResponseType(typeof(RecurringPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRecurringPlan(int planId, [FromBody] UpdateRecurringPlanRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ErrorResponse.ValidationError("Invalid update request / 无效的更新请求", errors));
        }

        // TODO: Get userId from authenticated user context
        const int userId = 1;

        try
        {
            var response = await _etfService.UpdateRecurringPlanAsync(userId, planId, request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Recurring plan update failed");
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
    }

    /// <summary>
    /// Delete recurring investment plan / 删除定投计划
    /// </summary>
    /// <param name="planId">Plan ID / 计划 ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>No content / 无内容</returns>
    /// <response code="204">Plan deleted / 计划已删除</response>
    /// <response code="404">Plan not found / 计划未找到</response>
    [HttpDelete("recurring-plans/{planId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecurringPlan(int planId, CancellationToken cancellationToken)
    {
        // TODO: Get userId from authenticated user context
        const int userId = 1;

        try
        {
            await _etfService.DeleteRecurringPlanAsync(userId, planId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Recurring plan deletion failed");
            return NotFound(new ErrorResponse("NOT_FOUND", ex.Message));
        }
    }

    #endregion
}
