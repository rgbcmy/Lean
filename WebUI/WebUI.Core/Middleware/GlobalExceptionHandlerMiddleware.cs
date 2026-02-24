using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Middleware;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 记录详细错误日志
        var requestId = context.TraceIdentifier;
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var endpoint = $"{context.Request.Method} {context.Request.Path}";

        _logger.LogError(exception,
            "Unhandled exception in request {RequestId} by user {UserId} on {Endpoint}",
            requestId, userId, endpoint);

        // 确定错误响应
        ErrorResponse errorResponse;
        int statusCode;

        switch (exception)
        {
            case ArgumentException argEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.InvalidParameter(
                    argEx.ParamName ?? "unknown",
                    argEx.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = ErrorResponse.Unauthorized();
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                errorResponse = ErrorResponse.NotFound("unknown", "请求的资源未找到");
                break;

            case InvalidOperationException invalidOpEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = ErrorResponse.ValidationError(invalidOpEx.Message);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                
                // 生产环境不暴露详细错误信息
                if (_environment.IsProduction())
                {
                    errorResponse = ErrorResponse.ServerError(
                        "服务器内部错误，请稍后重试",
                        new { requestId });
                }
                else
                {
                    // 开发环境返回详细信息
                    errorResponse = ErrorResponse.ServerError(
                        exception.Message,
                        new
                        {
                            requestId,
                            exceptionType = exception.GetType().Name,
                            stackTrace = exception.StackTrace
                        });
                }
                break;
        }

        // 设置响应
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = !_environment.IsProduction()
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}
