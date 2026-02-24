using FluentValidation;
using WebUI.Core.Models;

namespace WebUI.API.Validators;

/// <summary>
/// Validator for market order requests / 市价单请求验证器
/// </summary>
public class MarketOrderRequestValidator : AbstractValidator<MarketOrderRequest>
{
    public MarketOrderRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage("Stock symbol is required / 股票代码必填")
            .MaximumLength(20)
            .WithMessage("Symbol must not exceed 20 characters / 股票代码不能超过 20 个字符")
            .Matches("^[A-Za-z]+$")
            .WithMessage("Symbol must contain only letters / 股票代码只能包含字母");

        RuleFor(x => x.Side)
            .NotEmpty()
            .WithMessage("Order side is required / 买卖方向必填")
            .Must(x => x == "Buy" || x == "Sell")
            .WithMessage("Side must be 'Buy' or 'Sell' / 方向必须是 'Buy' 或 'Sell'");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0 / 数量必须大于 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Quantity must not exceed 1,000,000 / 数量不能超过 1,000,000")
            .Must(BeWholeNumber)
            .WithMessage("Fractional shares are not supported / 不支持碎股交易，请输入整数");
    }

    private static bool BeWholeNumber(int quantity)
    {
        return quantity == Math.Floor((decimal)quantity);
    }
}

/// <summary>
/// Validator for limit order requests / 限价单请求验证器
/// </summary>
public class LimitOrderRequestValidator : AbstractValidator<LimitOrderRequest>
{
    public LimitOrderRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage("Stock symbol is required / 股票代码必填")
            .MaximumLength(20)
            .WithMessage("Symbol must not exceed 20 characters / 股票代码不能超过 20 个字符")
            .Matches("^[A-Za-z]+$")
            .WithMessage("Symbol must contain only letters / 股票代码只能包含字母");

        RuleFor(x => x.Side)
            .NotEmpty()
            .WithMessage("Order side is required / 买卖方向必填")
            .Must(x => x == "Buy" || x == "Sell")
            .WithMessage("Side must be 'Buy' or 'Sell' / 方向必须是 'Buy' 或 'Sell'");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0 / 数量必须大于 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Quantity must not exceed 1,000,000 / 数量不能超过 1,000,000")
            .Must(BeWholeNumber)
            .WithMessage("Fractional shares are not supported / 不支持碎股交易，请输入整数");

        RuleFor(x => x.LimitPrice)
            .GreaterThan(0)
            .WithMessage("Limit price must be greater than 0 / 限价必须大于 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Limit price must not exceed $1,000,000 / 限价不能超过 $1,000,000");
    }

    private static bool BeWholeNumber(int quantity)
    {
        return quantity == Math.Floor((decimal)quantity);
    }
}

/// <summary>
/// Validator for order cost estimate requests / 订单成本预估请求验证器
/// </summary>
public class OrderCostEstimateRequestValidator : AbstractValidator<OrderCostEstimateRequest>
{
    public OrderCostEstimateRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .WithMessage("Stock symbol is required / 股票代码必填")
            .MaximumLength(20)
            .WithMessage("Symbol must not exceed 20 characters / 股票代码不能超过 20 个字符")
            .Matches("^[A-Za-z]+$")
            .WithMessage("Symbol must contain only letters / 股票代码只能包含字母");

        RuleFor(x => x.Side)
            .NotEmpty()
            .WithMessage("Order side is required / 买卖方向必填")
            .Must(x => x == "Buy" || x == "Sell")
            .WithMessage("Side must be 'Buy' or 'Sell' / 方向必须是 'Buy' 或 'Sell'");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0 / 数量必须大于 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Quantity must not exceed 1,000,000 / 数量不能超过 1,000,000");

        RuleFor(x => x.LimitPrice)
            .GreaterThan(0)
            .WithMessage("Limit price must be greater than 0 / 限价必须大于 0")
            .When(x => x.LimitPrice.HasValue)
            .LessThanOrEqualTo(1000000)
            .WithMessage("Limit price must not exceed $1,000,000 / 限价不能超过 $1,000,000")
            .When(x => x.LimitPrice.HasValue);
    }
}

/// <summary>
/// Validator for order query parameters / 订单查询参数验证器
/// </summary>
public class OrderQueryParametersValidator : AbstractValidator<OrderQueryParameters>
{
    private static readonly string[] AllowedStatuses = { "Pending", "Submitted", "PartiallyFilled", "Filled", "Cancelled", "Rejected" };
    private static readonly string[] AllowedSides = { "Buy", "Sell" };
    private static readonly string[] AllowedOrderTypes = { "Market", "Limit", "Stop", "StopLimit" };
    private static readonly string[] AllowedSortFields = { "CreatedAt", "Symbol", "Quantity", "Price", "Status" };

    public OrderQueryParametersValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => AllowedStatuses.Contains(status!))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)} / 状态必须是：{string.Join(", ", AllowedStatuses)}");

        RuleFor(x => x.Side)
            .Must(side => AllowedSides.Contains(side!))
            .When(x => !string.IsNullOrWhiteSpace(x.Side))
            .WithMessage("Side must be 'Buy' or 'Sell' / 方向必须是 'Buy' 或 'Sell'");

        RuleFor(x => x.OrderType)
            .Must(type => AllowedOrderTypes.Contains(type!))
            .When(x => !string.IsNullOrWhiteSpace(x.OrderType))
            .WithMessage($"OrderType must be one of: {string.Join(", ", AllowedOrderTypes)}");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0 / 页码必须大于 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0 / 每页数量必须大于 0")
            .LessThanOrEqualTo(200)
            .WithMessage("Page size must not exceed 200 / 每页数量不能超过 200");

        RuleFor(x => x.SortBy)
            .Must(field => AllowedSortFields.Contains(field!, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Start date must be before or equal to end date / 开始日期必须早于或等于结束日期");
    }
}
