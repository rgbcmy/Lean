namespace WebUI.Core.Models;

/// <summary>
/// 统一的 API 错误响应格式
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// 错误信息对象
    /// </summary>
    public ErrorDetail Error { get; set; }

    public ErrorResponse(string code, string message, object? details = null)
    {
        Error = new ErrorDetail(code, message, details);
    }

    public ErrorResponse(ErrorDetail error)
    {
        Error = error;
    }

    /// <summary>
    /// 创建验证错误响应
    /// </summary>
    public static ErrorResponse ValidationError(string message, object? details = null)
    {
        return new ErrorResponse("VALIDATION_ERROR", message, details);
    }

    /// <summary>
    /// 创建未授权错误响应
    /// </summary>
    public static ErrorResponse Unauthorized(string message = "未授权访问")
    {
        return new ErrorResponse("UNAUTHORIZED", message);
    }

    /// <summary>
    /// 创建未找到错误响应
    /// </summary>
    public static ErrorResponse NotFound(string resource, string message = "资源未找到")
    {
        return new ErrorResponse("NOT_FOUND", message, new { resource });
    }

    /// <summary>
    /// 创建服务器错误响应
    /// </summary>
    public static ErrorResponse ServerError(string message = "服务器内部错误", object? details = null)
    {
        return new ErrorResponse("INTERNAL_ERROR", message, details);
    }

    /// <summary>
    /// 创建参数错误响应
    /// </summary>
    public static ErrorResponse InvalidParameter(string parameterName, string message)
    {
        return new ErrorResponse("INVALID_PARAMETER", message, new { parameter = parameterName });
    }

    /// <summary>
    /// 创建速率限制错误响应
    /// </summary>
    public static ErrorResponse RateLimitExceeded(string message = "请求频率超过限制", DateTime? retryAfter = null)
    {
        return new ErrorResponse("RATE_LIMIT_EXCEEDED", message, new { retryAfter });
    }
}

/// <summary>
/// 错误详情
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// 错误代码（如 VALIDATION_ERROR, UNAUTHORIZED 等）
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 错误描述信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 错误附加详情（可选）
    /// </summary>
    public object? Details { get; set; }

    public ErrorDetail(string code, string message, object? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}
