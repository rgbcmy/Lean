using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Core.Models.Backtest;
using WebUI.Data.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Backtest management controller / 回测管理控制器
/// Provides backtest CRUD and execution APIs
/// 提供回测 CRUD 和执行 API
/// </summary>
[ApiController]
[Route("api/v1/backtests")]
[Authorize]
public class BacktestsController : ControllerBase
{
    private readonly ILogger<BacktestsController> _logger;
    private readonly IBacktestService _backtestService;
    private readonly IBacktestExecutionService _backtestExecutionService;
    private readonly IParameterOptimizationService _optimizationService;
    private readonly IBacktestExportService _exportService;
    private const int DefaultUserId = 1; // TODO: Get from JWT claims

    public BacktestsController(
        ILogger<BacktestsController> logger,
        IBacktestService backtestService,
        IBacktestExecutionService backtestExecutionService,
        IParameterOptimizationService optimizationService,
        IBacktestExportService exportService)
    {
        _logger = logger;
        _backtestService = backtestService;
        _backtestExecutionService = backtestExecutionService;
        _optimizationService = optimizationService;
        _exportService = exportService;
    }

    /// <summary>
    /// Create a new backtest / 创建新回测
    /// </summary>
    /// <param name="request">Backtest configuration / 回测配置</param>
    /// <response code="201">Backtest created / 回测已创建</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(BacktestDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBacktest([FromBody] CreateBacktestRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_FAILED", 
                $"Validation failed / 验证失败: {string.Join(", ", errors)}"));
        }

        try
        {
            var backtest = await _backtestService.CreateBacktestAsync(request, DefaultUserId);
            _logger.LogInformation("Backtest {BacktestId} created successfully", backtest.Id);
            return CreatedAtAction(nameof(GetBacktest), new { id = backtest.Id }, backtest);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to create backtest: {Message}", ex.Message);
            return BadRequest(new ErrorResponse("INVALID_REQUEST", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backtest");
            return StatusCode(500, new ErrorResponse("BACKTEST_CREATE_ERROR", 
                "Failed to create backtest / 创建回测失败"));
        }
    }

    /// <summary>
    /// Get all backtests / 获取所有回测
    /// </summary>
    /// <param name="strategyId">Filter by strategy ID / 按策略 ID 筛选</param>
    /// <param name="status">Filter by status (Pending, Running, Completed, Failed) / 按状态筛选</param>
    /// <response code="200">Backtest list / 回测列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(BacktestListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBacktests([FromQuery] int? strategyId = null, [FromQuery] string? status = null)
    {
        var result = await _backtestService.GetBacktestsAsync(DefaultUserId, strategyId, status);
        return Ok(result);
    }

    /// <summary>
    /// Get backtest by ID / 根据 ID 获取回测
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">Backtest details / 回测详情</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BacktestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBacktest(int id)
    {
        var backtest = await _backtestService.GetBacktestByIdAsync(id, DefaultUserId);
        
        if (backtest == null)
        {
            return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", "Backtest not found / 回测不存在"));
        }

        return Ok(backtest);
    }

    /// <summary>
    /// Delete backtest / 删除回测
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="204">Backtest deleted / 回测已删除</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBacktest(int id)
    {
        var deleted = await _backtestService.DeleteBacktestAsync(id, DefaultUserId);
        
        if (!deleted)
        {
            return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", "Backtest not found / 回测不存在"));
        }

        _logger.LogInformation("Backtest {BacktestId} deleted successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Start backtest execution / 开始回测执行
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">Backtest started / 回测已开始</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpPost("{id}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartBacktest(int id)
    {
        try
        {
            var backtest = await _backtestService.GetBacktestByIdAsync(id, DefaultUserId);
            
            if (backtest == null)
            {
                return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", "Backtest not found / 回测不存在"));
            }

            if (backtest.Status == "Running")
            {
                return BadRequest(new ErrorResponse("BACKTEST_ALREADY_RUNNING", "Backtest is already running / 回测已在运行中"));
            }

            await _backtestExecutionService.StartBacktestAsync(id, DefaultUserId);
            
            _logger.LogInformation("Backtest {BacktestId} started successfully", id);
            return Ok(new { message = "Backtest started successfully / 回测已开始" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to start backtest {BacktestId}: {Message}", id, ex.Message);
            return BadRequest(new ErrorResponse("BACKTEST_START_FAILED", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting backtest {BacktestId}", id);
            return StatusCode(500, new ErrorResponse("BACKTEST_START_ERROR", "Failed to start backtest / 启动回测失败"));
        }
    }

    /// <summary>
    /// Stop backtest execution / 停止回测执行
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">Backtest stopped / 回测已停止</response>
    /// <response code="404">Backtest not found or not running / 回测不存在或未运行</response>
    [HttpPost("{id}/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StopBacktest(int id)
    {
        try
        {
            if (!_backtestExecutionService.IsBacktestRunning(id))
            {
                return BadRequest(new ErrorResponse("BACKTEST_NOT_RUNNING", "Backtest is not running / 回测未运行"));
            }

            await _backtestExecutionService.StopBacktestAsync(id, DefaultUserId);
            
            _logger.LogInformation("Backtest {BacktestId} stopped successfully", id);
            return Ok(new { message = "Backtest stopped successfully / 回测已停止" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to stop backtest {BacktestId}: {Message}", id, ex.Message);
            return BadRequest(new ErrorResponse("BACKTEST_STOP_FAILED", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping backtest {BacktestId}", id);
            return StatusCode(500, new ErrorResponse("BACKTEST_STOP_ERROR", "Failed to stop backtest / 停止回测失败"));
        }
    }

    /// <summary>
    /// Get backtest execution status / 获取回测执行状态
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">Backtest status / 回测状态</response>
    /// <response code="404">Backtest not running / 回测未运行</response>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(BacktestStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBacktestStatus(int id)
    {
        var status = await _backtestExecutionService.GetBacktestStatusAsync(id);
        
        if (status == null)
        {
            return NotFound(new ErrorResponse("BACKTEST_NOT_RUNNING", "Backtest is not running / 回测未运行"));
        }

        return Ok(status);
    }

    /// <summary>
    /// Compare multiple backtests / 对比多个回测
    /// </summary>
    /// <param name="request">Backtest IDs to compare / 要对比的回测 ID</param>
    /// <response code="200">Comparison results / 对比结果</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(CompareBacktestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompareBacktests([FromBody] CompareBacktestsRequest request)
    {
        try
        {
            var comparison = await _backtestService.CompareBacktestsAsync(request.BacktestIds, DefaultUserId);
            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid comparison request: {Message}", ex.Message);
            return BadRequest(new ErrorResponse("INVALID_COMPARISON_REQUEST", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing backtests");
            return StatusCode(500, new ErrorResponse("COMPARISON_ERROR", "Failed to compare backtests / 对比回测失败"));
        }
    }

    /// <summary>
    /// Start parameter optimization / 开始参数优化
    /// </summary>
    /// <param name="request">Optimization configuration / 优化配置</param>
    /// <response code="201">Optimization started / 优化已开始</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost("optimize")]
    [ProducesResponseType(typeof(OptimizeParametersResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OptimizeParameters([FromBody] OptimizeParametersRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_FAILED",
                $"Validation failed / 验证失败: {string.Join(", ", errors)}"));
        }

        try
        {
            var optimization = await _optimizationService.StartOptimizationAsync(request, DefaultUserId);
            _logger.LogInformation("Optimization {OptimizationId} started successfully", optimization.OptimizationId);
            return CreatedAtAction(nameof(GetOptimization), new { id = optimization.OptimizationId }, optimization);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to start optimization: {Message}", ex.Message);
            return BadRequest(new ErrorResponse("INVALID_REQUEST", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting optimization");
            return StatusCode(500, new ErrorResponse("OPTIMIZATION_START_ERROR",
                "Failed to start optimization / 启动优化失败"));
        }
    }

    /// <summary>
    /// Get optimization by ID / 根据 ID 获取优化
    /// </summary>
    /// <param name="id">Optimization ID / 优化 ID</param>
    /// <response code="200">Optimization details / 优化详情</response>
    /// <response code="404">Optimization not found / 优化不存在</response>
    [HttpGet("optimize/{id}")]
    [ProducesResponseType(typeof(OptimizeParametersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOptimization(int id)
    {
        var optimization = await _optimizationService.GetOptimizationAsync(id, DefaultUserId);

        if (optimization == null)
        {
            return NotFound(new ErrorResponse("OPTIMIZATION_NOT_FOUND", "Optimization not found / 优化不存在"));
        }

        return Ok(optimization);
    }

    /// <summary>
    /// Get all optimizations / 获取所有优化
    /// </summary>
    /// <param name="strategyId">Filter by strategy ID / 按策略 ID 筛选</param>
    /// <response code="200">Optimization list / 优化列表</response>
    [HttpGet("optimize")]
    [ProducesResponseType(typeof(OptimizeParametersResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptimizations([FromQuery] int? strategyId = null)
    {
        var optimizations = await _optimizationService.GetOptimizationsAsync(DefaultUserId, strategyId);
        return Ok(optimizations);
    }

    /// <summary>
    /// Cancel optimization / 取消优化
    /// </summary>
    /// <param name="id">Optimization ID / 优化 ID</param>
    /// <response code="200">Optimization cancelled / 优化已取消</response>
    /// <response code="404">Optimization not found / 优化不存在</response>
    [HttpPost("optimize/{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOptimization(int id)
    {
        try
        {
            await _optimizationService.CancelOptimizationAsync(id, DefaultUserId);
            _logger.LogInformation("Optimization {OptimizationId} cancelled", id);
            return Ok(new { message = "Optimization cancelled successfully / 优化已取消" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to cancel optimization {OptimizationId}: {Message}", id, ex.Message);
            return BadRequest(new ErrorResponse("OPTIMIZATION_CANCEL_FAILED", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling optimization {OptimizationId}", id);
            return StatusCode(500, new ErrorResponse("OPTIMIZATION_CANCEL_ERROR",
                "Failed to cancel optimization / 取消优化失败"));
        }
    }

    /// <summary>
    /// Export backtest as JSON / 导出回测为 JSON
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">JSON file / JSON 文件</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpGet("{id}/export/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportBacktestAsJson(int id)
    {
        try
        {
            var json = await _exportService.ExportAsJsonAsync(id, DefaultUserId);
            return File(json, "application/json", $"backtest_{id}.json");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to export backtest {BacktestId}: {Message}", id, ex.Message);
            return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting backtest {BacktestId} as JSON", id);
            return StatusCode(500, new ErrorResponse("EXPORT_ERROR", "Failed to export backtest / 导出回测失败"));
        }
    }

    /// <summary>
    /// Export backtest trades as CSV / 导出回测交易记录为 CSV
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">CSV file / CSV 文件</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpGet("{id}/export/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportBacktestTradesAsCsv(int id)
    {
        try
        {
            var csv = await _exportService.ExportTradesAsCsvAsync(id, DefaultUserId);
            return File(csv, "text/csv", $"backtest_{id}_trades.csv");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to export backtest {BacktestId}: {Message}", id, ex.Message);
            return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting backtest {BacktestId} as CSV", id);
            return StatusCode(500, new ErrorResponse("EXPORT_ERROR", "Failed to export backtest / 导出回测失败"));
        }
    }

    /// <summary>
    /// Export backtest report as HTML / 导出回测报告为 HTML
    /// </summary>
    /// <param name="id">Backtest ID / 回测 ID</param>
    /// <response code="200">HTML file / HTML 文件</response>
    /// <response code="404">Backtest not found / 回测不存在</response>
    [HttpGet("{id}/export/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportBacktestAsHtml(int id)
    {
        try
        {
            var html = await _exportService.ExportAsHtmlAsync(id, DefaultUserId);
            return File(html, "text/html", $"backtest_{id}_report.html");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to export backtest {BacktestId}: {Message}", id, ex.Message);
            return NotFound(new ErrorResponse("BACKTEST_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting backtest {BacktestId} as HTML", id);
            return StatusCode(500, new ErrorResponse("EXPORT_ERROR", "Failed to export backtest / 导出回测失败"));
        }
    }
}
