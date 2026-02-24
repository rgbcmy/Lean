using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Core.Models;
using WebUI.Data.Services;

namespace WebUI.API.Controllers;

/// <summary>
/// Strategy management controller / 策略管理控制器
/// Provides strategy CRUD, version control, performance, and execution APIs
/// 提供策略 CRUD、版本控制、性能和执行 API
/// </summary>
[ApiController]
[Route("api/v1/strategies")]
[Authorize]
public class StrategiesController : ControllerBase
{
    private readonly ILogger<StrategiesController> _logger;
    private readonly IStrategyService _strategyService;
    private readonly IStrategyExecutionService _executionService;
    private const int DefaultUserId = 1; // TODO: Get from JWT claims

    public StrategiesController(
        ILogger<StrategiesController> logger,
        IStrategyService strategyService,
        IStrategyExecutionService executionService)
    {
        _logger = logger;
        _strategyService = strategyService;
        _executionService = executionService;
    }

    /// <summary>
    /// Get all strategies / 获取所有策略
    /// </summary>
    /// <param name="status">Filter by status (active/inactive) / 按状态筛选</param>
    /// <response code="200">Strategy list / 策略列表</response>
    [HttpGet]
    [ProducesResponseType(typeof(StrategyListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStrategies([FromQuery] string? status = null)
    {
        var result = await _strategyService.GetStrategiesAsync(DefaultUserId, status);
        return Ok(result);
    }

    /// <summary>
    /// Get strategy by ID / 根据 ID 获取策略
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">Strategy details / 策略详情</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategy(int id)
    {
        var strategy = await _strategyService.GetStrategyByIdAsync(id, DefaultUserId);
        
        if (strategy == null)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
        }

        return Ok(strategy);
    }

    /// <summary>
    /// Create a new strategy / 创建新策略
    /// </summary>
    /// <param name="request">Create strategy request / 创建策略请求</param>
    /// <response code="201">Strategy created / 策略已创建</response>
    /// <response code="400">Invalid request / 请求无效</response>
    [HttpPost]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStrategy([FromBody] CreateStrategyRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid request / 请求无效", errors));
        }

        try
        {
            var strategy = await _strategyService.CreateStrategyAsync(DefaultUserId, request);
            
            _logger.LogInformation("Created strategy {StrategyId} '{Name}'", strategy.Id, strategy.Name);
            
            return CreatedAtAction(nameof(GetStrategy), new { id = strategy.Id }, strategy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("STRATEGY_CREATE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Update strategy / 更新策略
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="request">Update strategy request / 更新策略请求</param>
    /// <response code="200">Strategy updated / 策略已更新</response>
    /// <response code="400">Invalid request or strategy is active / 请求无效或策略正在运行</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStrategy(int id, [FromBody] UpdateStrategyRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid request / 请求无效", errors));
        }

        try
        {
            var strategy = await _strategyService.UpdateStrategyAsync(id, DefaultUserId, request);
            
            if (strategy == null)
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
            }

            _logger.LogInformation("Updated strategy {StrategyId}", id);
            
            return Ok(strategy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("STRATEGY_UPDATE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Delete strategy / 删除策略
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="204">Strategy deleted / 策略已删除</response>
    /// <response code="400">Cannot delete active strategy / 无法删除运行中的策略</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStrategy(int id)
    {
        try
        {
            var deleted = await _strategyService.DeleteStrategyAsync(id, DefaultUserId);
            
            if (!deleted)
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
            }

            _logger.LogInformation("Deleted strategy {StrategyId}", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("STRATEGY_DELETE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Clone strategy / 克隆策略
    /// </summary>
    /// <param name="id">Source strategy ID / 源策略 ID</param>
    /// <param name="request">Clone request / 克隆请求</param>
    /// <response code="201">Strategy cloned / 策略已克隆</response>
    /// <response code="400">Invalid request / 请求无效</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPost("{id}/clone")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloneStrategy(int id, [FromBody] CloneStrategyRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid request / 请求无效", errors));
        }

        try
        {
            var strategy = await _strategyService.CloneStrategyAsync(id, DefaultUserId, request);
            
            if (strategy == null)
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
            }

            _logger.LogInformation("Cloned strategy {SourceId} to {ClonedId}", id, strategy.Id);
            
            return CreatedAtAction(nameof(GetStrategy), new { id = strategy.Id }, strategy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("STRATEGY_CLONE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Get strategy version history / 获取策略版本历史
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">Version list / 版本列表</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/versions")]
    [ProducesResponseType(typeof(List<StrategyVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyVersions(int id)
    {
        var versions = await _strategyService.GetStrategyVersionsAsync(id, DefaultUserId);
        
        if (versions.Count == 0)
        {
            // Check if strategy exists
            var strategy = await _strategyService.GetStrategyByIdAsync(id, DefaultUserId);
            if (strategy == null)
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
            }
        }

        return Ok(versions);
    }

    /// <summary>
    /// Rollback to previous version / 回滚到历史版本
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="versionId">Version ID / 版本 ID</param>
    /// <response code="200">Strategy rolled back / 策略已回滚</response>
    /// <response code="400">Cannot rollback active strategy / 无法回滚运行中的策略</response>
    /// <response code="404">Strategy or version not found / 策略或版本不存在</response>
    [HttpPost("{id}/versions/{versionId}/rollback")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RollbackToVersion(int id, int versionId)
    {
        try
        {
            var strategy = await _strategyService.RollbackToVersionAsync(id, versionId, DefaultUserId);
            
            if (strategy == null)
            {
                return NotFound(new ErrorResponse("STRATEGY_OR_VERSION_NOT_FOUND", 
                    "Strategy or version not found / 策略或版本不存在"));
            }

            _logger.LogInformation("Rolled back strategy {StrategyId} to version {VersionId}", id, versionId);
            
            return Ok(strategy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("ROLLBACK_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Update strategy tags / 更新策略标签
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="tags">Comma-separated tags / 逗号分隔的标签</param>
    /// <response code="200">Tags updated / 标签已更新</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPatch("{id}/tags")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStrategyTags(int id, [FromBody] string? tags)
    {
        var strategy = await _strategyService.UpdateStrategyTagsAsync(id, DefaultUserId, tags);
        
        if (strategy == null)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
        }

        _logger.LogInformation("Updated tags for strategy {StrategyId}", id);
        
        return Ok(strategy);
    }

    /// <summary>
    /// Get strategy performance summary / 获取策略性能摘要
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">Performance metrics / 性能指标</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/performance")]
    [ProducesResponseType(typeof(StrategyPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyPerformance(int id)
    {
        var performance = await _strategyService.GetStrategyPerformanceAsync(id, DefaultUserId);
        
        if (performance == null)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
        }

        return Ok(performance);
    }

    /// <summary>
    /// Upload strategy code file / 上传策略代码文件
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="file">Code file (.py or .cs) / 代码文件</param>
    /// <response code="200">File uploaded / 文件已上传</response>
    /// <response code="400">Invalid file or strategy is active / 文件无效或策略正在运行</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPost("{id}/upload")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadStrategyCode(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "File is required / 文件必填"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var filePath = await _strategyService.UploadStrategyCodeAsync(id, DefaultUserId, file.FileName, stream);
            
            _logger.LogInformation("Uploaded code file for strategy {StrategyId}: {FileName}", id, file.FileName);
            
            return Ok(new { filePath, message = "File uploaded successfully / 文件上传成功" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("UPLOAD_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Export strategy as ZIP package / 导出策略为 ZIP 包
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">ZIP file / ZIP 文件</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportStrategy(int id)
    {
        var zipBytes = await _strategyService.ExportStrategyAsync(id, DefaultUserId);
        
        if (zipBytes == null)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", "Strategy not found / 策略不存在"));
        }

        var strategy = await _strategyService.GetStrategyByIdAsync(id, DefaultUserId);
        var fileName = $"{strategy!.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
        
        _logger.LogInformation("Exported strategy {StrategyId} as {FileName}", id, fileName);
        
        return File(zipBytes, "application/zip", fileName);
    }

    /// <summary>
    /// Import strategy from ZIP package / 从 ZIP 包导入策略
    /// </summary>
    /// <param name="file">Strategy ZIP file / 策略 ZIP 文件</param>
    /// <response code="201">Strategy imported / 策略已导入</response>
    /// <response code="400">Invalid file / 文件无效</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(StrategyDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportStrategy(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "File is required / 文件必填"));
        }

        if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Only ZIP files are supported / 仅支持 ZIP 文件"));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var strategy = await _strategyService.ImportStrategyAsync(DefaultUserId, stream);
            
            if (strategy == null)
            {
                return BadRequest(new ErrorResponse("IMPORT_FAILED", "Failed to import strategy / 导入策略失败"));
            }

            _logger.LogInformation("Imported strategy {StrategyId} '{Name}'", strategy.Id, strategy.Name);
            
            return CreatedAtAction(nameof(GetStrategy), new { id = strategy.Id }, strategy);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse("IMPORT_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Generate Lean configuration file / 生成 Lean 配置文件
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">Configuration JSON / 配置 JSON</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/config")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateLeanConfig(int id)
    {
        try
        {
            var configJson = await _strategyService.GenerateLeanConfigAsync(id, DefaultUserId);
            return Ok(new { config = configJson });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
        }
    }

    // ==================== Strategy Execution Endpoints ====================

    /// <summary>
    /// Start strategy execution / 启动策略执行
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="request">Start request / 启动请求</param>
    /// <response code="200">Strategy started / 策略已启动</response>
    /// <response code="400">Strategy already running or invalid config / 策略已运行或配置无效</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(StrategyExecutionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartStrategy(int id, [FromBody] StartStrategyRequest? request = null)
    {
        try
        {
            request ??= new StartStrategyRequest();
            var execution = await _executionService.StartStrategyAsync(id, DefaultUserId, request);
            
            _logger.LogInformation("Started strategy {StrategyId} with execution {ExecutionId}", id, execution.Id);
            
            return Ok(execution);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found") || ex.Message.Contains("不存在"))
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
            }
            return BadRequest(new ErrorResponse("START_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Stop strategy execution / 停止策略执行
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="request">Stop request / 停止请求</param>
    /// <response code="204">Strategy stopped / 策略已停止</response>
    /// <response code="400">Strategy not running / 策略未运行</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpPost("{id}/stop")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopStrategy(int id, [FromBody] StopStrategyRequest? request = null)
    {
        try
        {
            request ??= new StopStrategyRequest();
            await _executionService.StopStrategyAsync(id, DefaultUserId, request);
            
            _logger.LogInformation("Stopped strategy {StrategyId}", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found") || ex.Message.Contains("不存在"))
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
            }
            return BadRequest(new ErrorResponse("STOP_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Get strategy runtime status / 获取策略运行时状态
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="200">Runtime status / 运行时状态</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(StrategyRuntimeStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyStatus(int id)
    {
        try
        {
            var status = await _executionService.GetStrategyRuntimeStatusAsync(id, DefaultUserId);
            return Ok(status);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
        }
    }

    /// <summary>
    /// Get strategy execution history / 获取策略执行历史
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="page">Page number / 页码</param>
    /// <param name="pageSize">Page size / 每页数量</param>
    /// <response code="200">Execution list / 执行列表</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    [HttpGet("{id}/executions")]
    [ProducesResponseType(typeof(StrategyExecutionListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyExecutions(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var executions = await _executionService.GetStrategyExecutionsAsync(id, DefaultUserId, page, pageSize);
            return Ok(executions);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
        }
    }

    /// <summary>
    /// Get execution details / 获取执行详情
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="executionId">Execution ID / 执行 ID</param>
    /// <response code="200">Execution details / 执行详情</response>
    /// <response code="404">Execution not found / 执行不存在</response>
    [HttpGet("{id}/executions/{executionId}")]
    [ProducesResponseType(typeof(StrategyExecutionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionDetails(int id, int executionId)
    {
        var execution = await _executionService.GetExecutionDetailsAsync(executionId, DefaultUserId);
        
        if (execution == null || execution.StrategyId != id)
        {
            return NotFound(new ErrorResponse("EXECUTION_NOT_FOUND", "Execution not found / 执行不存在"));
        }

        return Ok(execution);
    }

    /// <summary>
    /// Schedule strategy execution / 定时执行策略
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <param name="request">Schedule request / 调度请求</param>
    /// <response code="204">Schedule created / 调度已创建</response>
    /// <response code="400">Invalid request / 请求无效</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    /// <response code="501">Not implemented yet / 尚未实现</response>
    [HttpPost("{id}/schedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> ScheduleStrategy(int id, [FromBody] ScheduleStrategyExecutionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Invalid request / 请求无效", errors));
        }

        try
        {
            await _executionService.ScheduleStrategyAsync(id, DefaultUserId, request);
            
            _logger.LogInformation("Scheduled strategy {StrategyId} with type {ScheduleType}", id, request.ScheduleType);
            
            return NoContent();
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new ErrorResponse("NOT_IMPLEMENTED", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found") || ex.Message.Contains("不存在"))
            {
                return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
            }
            return BadRequest(new ErrorResponse("SCHEDULE_FAILED", ex.Message));
        }
    }

    /// <summary>
    /// Cancel scheduled execution / 取消定时执行
    /// </summary>
    /// <param name="id">Strategy ID / 策略 ID</param>
    /// <response code="204">Schedule canceled / 调度已取消</response>
    /// <response code="404">Strategy not found / 策略不存在</response>
    /// <response code="501">Not implemented yet / 尚未实现</response>
    [HttpDelete("{id}/schedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> CancelSchedule(int id)
    {
        try
        {
            await _executionService.CancelScheduleAsync(id, DefaultUserId);
            
            _logger.LogInformation("Canceled schedule for strategy {StrategyId}", id);
            
            return NoContent();
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, 
                new ErrorResponse("NOT_IMPLEMENTED", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorResponse("STRATEGY_NOT_FOUND", ex.Message));
        }
    }
}
