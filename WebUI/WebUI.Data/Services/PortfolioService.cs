using System.Text;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;
using WebUI.Core.Services;
using WebUI.Data.Entities;
using WebUI.Data.Repositories;

namespace WebUI.Data.Services
{
    /// <summary>
    /// Portfolio management service implementation
    /// </summary>
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PortfolioService> _logger;
        private readonly IMarketDataService _marketDataService;

        public PortfolioService(
            IUnitOfWork unitOfWork,
            ILogger<PortfolioService> logger,
            IMarketDataService marketDataService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _marketDataService = marketDataService;
        }

        public async Task<PortfolioPositionsResponse> GetPositionsAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting positions for broker account {BrokerAccountId}", brokerAccountId);

            var positions = await _unitOfWork.Positions.GetByBrokerAccountIdAsync(brokerAccountId, cancellationToken);
            var positionsList = positions.ToList();

            // Update prices from market data
            await UpdatePositionPricesInternalAsync(positionsList, cancellationToken);

            var response = new PortfolioPositionsResponse
            {
                Positions = positionsList.Select(MapToPositionResponse).ToList()
            };

            CalculateTotals(response);

            // Get cash balance from broker account
            var account = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);
            response.CashBalance = account?.CashBalance ?? 0;

            return response;
        }

        public async Task<PortfolioPositionsResponse> GetFilteredPositionsAsync(int brokerAccountId, PositionQueryOptions options, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting filtered positions for broker account {BrokerAccountId}", brokerAccountId);

            var positions = await _unitOfWork.Positions.GetFilteredPositionsAsync(
                brokerAccountId,
                options.SymbolFilter,
                options.OnlyProfitable,
                options.OnlyLosing,
                options.SortBy,
                options.SortDirection,
                cancellationToken);

            var positionsList = positions.ToList();

            // Update prices from market data
            await UpdatePositionPricesInternalAsync(positionsList, cancellationToken);

            var response = new PortfolioPositionsResponse
            {
                Positions = positionsList.Select(MapToPositionResponse).ToList()
            };

            CalculateTotals(response);

            // Get cash balance from broker account
            var account = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);
            response.CashBalance = account?.CashBalance ?? 0;

            return response;
        }

        public async Task<PositionDetailResponse?> GetPositionDetailAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting position detail for symbol {Symbol}", symbol);

            var position = await _unitOfWork.Positions.GetBySymbolAsync(brokerAccountId, symbol, cancellationToken);
            if (position == null || position.Quantity == 0)
            {
                return null;
            }

            // Update price from market data
            await UpdatePositionPricesInternalAsync(new List<Position> { position }, cancellationToken);

            var response = new PositionDetailResponse
            {
                Id = position.Id,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AverageCost = position.AverageCost,
                CurrentPrice = position.CurrentPrice,
                UnrealizedPnL = position.UnrealizedPnL,
                RealizedPnL = position.RealizedPnL,
                FirstPurchaseDate = position.FirstPurchaseDate,
                LastUpdatedAt = position.LastUpdatedAt,
                TradeHistory = await GetTradeHistoryAsync(brokerAccountId, symbol, cancellationToken)
            };

            return response;
        }

        public async Task<OrderResponse> ClosePositionAsync(int brokerAccountId, string symbol, ClosePositionRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Closing position {Symbol} for broker account {BrokerAccountId}", symbol, brokerAccountId);

            var position = await _unitOfWork.Positions.GetBySymbolAsync(brokerAccountId, symbol, cancellationToken);
            if (position == null || position.Quantity == 0)
            {
                throw new InvalidOperationException($"No position found for symbol {symbol}");
            }

            // Determine quantity to close
            int quantityToClose = request.Quantity ?? position.Quantity;
            if (quantityToClose > position.Quantity)
            {
                throw new InvalidOperationException($"Cannot close {quantityToClose} shares, only {position.Quantity} available");
            }

            // Create sell order
            // Note: This would integrate with the actual trading service
            // For now, we'll create a mock order response
            var order = new Order
            {
                BrokerAccountId = brokerAccountId,
                Symbol = symbol,
                Side = "Sell",
                OrderType = request.OrderType,
                Quantity = quantityToClose,
                LimitPrice = request.LimitPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created sell order {OrderId} to close position {Symbol}", order.Id, symbol);

            return new OrderResponse
            {
                OrderId = order.Id,
                Symbol = order.Symbol,
                Side = order.Side,
                OrderType = order.OrderType,
                Quantity = order.Quantity,
                LimitPrice = order.LimitPrice,
                Status = order.Status,
                SubmittedAt = order.CreatedAt
            };
        }

        public async Task<PortfolioAllocationResponse> GetPortfolioAllocationAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting portfolio allocation for broker account {BrokerAccountId}", brokerAccountId);

            var positions = await GetPositionsAsync(brokerAccountId, cancellationToken);

            decimal totalValue = positions.TotalPortfolioValue;

            var response = new PortfolioAllocationResponse
            {
                TotalValue = totalValue,
                Positions = positions.Positions.Select(p => new AllocationItem
                {
                    Name = p.Symbol,
                    Value = p.MarketValue,
                    Percentage = totalValue > 0 ? (p.MarketValue / totalValue) * 100 : 0,
                    UnrealizedPnL = p.UnrealizedPnL
                }).ToList(),
                Cash = new AllocationItem
                {
                    Name = "Cash",
                    Value = positions.CashBalance,
                    Percentage = totalValue > 0 ? (positions.CashBalance / totalValue) * 100 : 0,
                    UnrealizedPnL = 0
                }
            };

            return response;
        }

        public async Task<EquityCurveResponse> GetEquityCurveAsync(int brokerAccountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting equity curve for broker account {BrokerAccountId}", brokerAccountId);

            var account = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);
            if (account == null)
                throw new InvalidOperationException($"Broker account {brokerAccountId} not found");

            var positions = await GetPositionsAsync(brokerAccountId, cancellationToken);
            decimal currentEquity = positions.TotalPortfolioValue;
            decimal cashBalance = positions.CashBalance;

            // Generate data points from filled orders + current positions
            var orders = await _unitOfWork.Orders.GetByBrokerAccountIdAsync(brokerAccountId, cancellationToken);
            var filledOrders = orders
                .Where(o => o.BrokerAccountId == brokerAccountId && o.Status == "Filled" && (o.FilledAt ?? o.CreatedAt) != default)
                .OrderBy(o => o.FilledAt ?? o.CreatedAt)
                .ToList();

            var effectiveStart = startDate ?? (filledOrders.Any()
                ? (filledOrders.First().FilledAt ?? filledOrders.First().CreatedAt).AddDays(-1)
                : DateTime.UtcNow.AddMonths(-3));
            var effectiveEnd = endDate ?? DateTime.UtcNow;

            var dataPoints = new List<EquityCurvePoint>();

            if (!filledOrders.Any())
            {
                // No orders — return a flat line showing current equity
                int days = Math.Max(1, (int)(effectiveEnd - effectiveStart).TotalDays);
                for (int d = 0; d <= Math.Min(days, 90); d++)
                {
                    dataPoints.Add(new EquityCurvePoint
                    {
                        Date = effectiveStart.AddDays(d),
                        Equity = currentEquity,
                        Cash = cashBalance,
                        PositionsValue = positions.TotalMarketValue,
                        CumulativeReturn = 0
                    });
                }
            }
            else
            {
                // Build a daily equity curve from order history
                // Seed initial capital: estimate from the first buy order's value + cash
                decimal runningCash = cashBalance + positions.TotalCostBasis; // rough initial capital
                decimal initialEquity = runningCash;
                decimal runningPositionsCostBasis = 0;

                // Group orders by date
                var ordersByDate = filledOrders
                    .GroupBy(o => (o.FilledAt ?? o.CreatedAt).Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var current = effectiveStart.Date;
                var end = effectiveEnd.Date;

                while (current <= end)
                {
                    if (ordersByDate.TryGetValue(current, out var dayOrders))
                    {
                        foreach (var order in dayOrders)
                        {
                            decimal fillPrice = order.FilledPrice ?? order.LimitPrice ?? 0;
                            decimal value = fillPrice * (order.FilledQuantity > 0 ? order.FilledQuantity : order.Quantity);
                            if (order.Side == "Buy")
                            {
                                runningCash -= value;
                                runningPositionsCostBasis += value;
                            }
                            else if (order.Side == "Sell")
                            {
                                runningCash += value;
                                runningPositionsCostBasis = Math.Max(0, runningPositionsCostBasis - value);
                            }
                        }
                    }

                    decimal equity = runningCash + runningPositionsCostBasis;
                    decimal cumulativeReturn = initialEquity > 0 ? ((equity - initialEquity) / initialEquity) * 100 : 0;

                    // Only add one point per day, skip weekends for cleanliness (optional)
                    dataPoints.Add(new EquityCurvePoint
                    {
                        Date = current,
                        Equity = Math.Max(equity, 0),
                        Cash = Math.Max(runningCash, 0),
                        PositionsValue = Math.Max(runningPositionsCostBasis, 0),
                        CumulativeReturn = cumulativeReturn
                    });

                    current = current.AddDays(1);
                }

                // Correct the last point to use actual current values
                if (dataPoints.Any())
                {
                    var last = dataPoints[^1];
                    last.Equity = currentEquity;
                    last.Cash = cashBalance;
                    last.PositionsValue = positions.TotalMarketValue;
                    if (initialEquity > 0)
                        last.CumulativeReturn = ((currentEquity - initialEquity) / initialEquity) * 100;
                }

                initialEquity = dataPoints.Any() ? dataPoints[0].Equity : currentEquity;
            }

            decimal maxDrawdown = 0;
            if (dataPoints.Count > 1)
            {
                decimal peak = dataPoints[0].Equity;
                foreach (var pt in dataPoints)
                {
                    if (pt.Equity > peak) peak = pt.Equity;
                    decimal dd = peak > 0 ? ((pt.Equity - peak) / peak) * 100 : 0;
                    if (dd < maxDrawdown) maxDrawdown = dd;
                }
            }

            decimal initEq = dataPoints.Any() ? dataPoints[0].Equity : currentEquity;
            decimal totalReturn = initEq > 0 ? ((currentEquity - initEq) / initEq) * 100 : 0;

            return new EquityCurveResponse
            {
                DataPoints = dataPoints,
                InitialEquity = initEq,
                CurrentEquity = currentEquity,
                TotalReturn = totalReturn,
                MaxDrawdown = maxDrawdown
            };
        }

        public async Task UpdatePositionPricesAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating position prices for broker account {BrokerAccountId}", brokerAccountId);

            var positions = await _unitOfWork.Positions.GetByBrokerAccountIdAsync(brokerAccountId, cancellationToken);
            var positionsList = positions.ToList();

            await UpdatePositionPricesInternalAsync(positionsList, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<byte[]> ExportPositionsToCsvAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Exporting positions to CSV for broker account {BrokerAccountId}", brokerAccountId);

            var positions = await GetPositionsAsync(brokerAccountId, cancellationToken);

            var csv = new StringBuilder();
            csv.AppendLine("Symbol,Quantity,Average Cost,Current Price,Market Value,Unrealized P&L,Unrealized P&L %,Realized P&L,Holding Days");

            foreach (var position in positions.Positions)
            {
                csv.AppendLine($"{position.Symbol},{position.Quantity},{position.AverageCost:F2},{position.CurrentPrice:F2},{position.MarketValue:F2},{position.UnrealizedPnL:F2},{position.UnrealizedPnLPercent:F2},{position.RealizedPnL:F2},{position.HoldingDays}");
            }

            csv.AppendLine();
            csv.AppendLine($"Total Market Value,,,,{positions.TotalMarketValue:F2}");
            csv.AppendLine($"Total Unrealized P&L,,,,{positions.TotalUnrealizedPnL:F2}");
            csv.AppendLine($"Total Realized P&L,,,,{positions.TotalRealizedPnL:F2}");
            csv.AppendLine($"Cash Balance,,,,{positions.CashBalance:F2}");
            csv.AppendLine($"Total Portfolio Value,,,,{positions.TotalPortfolioValue:F2}");

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportPositionsToExcelAsync(int brokerAccountId, CancellationToken cancellationToken = default)
        {
            // TODO: Implement Excel export using a library like EPPlus or ClosedXML
            // For now, return CSV
            _logger.LogWarning("Excel export not yet implemented, returning CSV instead");
            return await ExportPositionsToCsvAsync(brokerAccountId, cancellationToken);
        }

        // Private helper methods

        private async Task UpdatePositionPricesInternalAsync(List<Position> positions, CancellationToken cancellationToken)
        {
            foreach (var position in positions)
            {
                try
                {
                    // Get latest price from market data service
                    var quote = await _marketDataService.GetQuoteAsync(position.Symbol, cancellationToken);
                    if (quote != null)
                    {
                        position.CurrentPrice = quote.LastPrice;
                        position.UnrealizedPnL = (position.CurrentPrice - position.AverageCost) * position.Quantity;
                        position.LastUpdatedAt = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update price for symbol {Symbol}", position.Symbol);
                }
            }
        }

        private PositionResponse MapToPositionResponse(Position position)
        {
            return new PositionResponse
            {
                Id = position.Id,
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AverageCost = position.AverageCost,
                CurrentPrice = position.CurrentPrice,
                UnrealizedPnL = position.UnrealizedPnL,
                RealizedPnL = position.RealizedPnL,
                FirstPurchaseDate = position.FirstPurchaseDate,
                LastUpdatedAt = position.LastUpdatedAt
            };
        }

        private void CalculateTotals(PortfolioPositionsResponse response)
        {
            response.TotalMarketValue = response.Positions.Sum(p => p.MarketValue);
            response.TotalCostBasis = response.Positions.Sum(p => p.CostBasis);
            response.TotalUnrealizedPnL = response.Positions.Sum(p => p.UnrealizedPnL);
            response.TotalRealizedPnL = response.Positions.Sum(p => p.RealizedPnL);
        }

        private async Task<List<TradeHistoryItem>> GetTradeHistoryAsync(int brokerAccountId, string symbol, CancellationToken cancellationToken)
        {
            // Get trade history from orders
            var orders = await _unitOfWork.Orders.GetOrdersBySymbolAsync(brokerAccountId, symbol, cancellationToken);

            return orders
                .Where(o => o.Status == "Filled")
                .Select(o => new TradeHistoryItem
                {
                    TradeDate = o.FilledAt ?? o.CreatedAt,
                    Side = o.Side,
                    Quantity = o.FilledQuantity > 0 ? o.FilledQuantity : o.Quantity,
                    Price = o.FilledPrice ?? 0,
                    RealizedPnL = o.Side == "Sell" ? CalculateRealizedPnL(o) : null
                })
                .OrderByDescending(t => t.TradeDate)
                .ToList();
        }

        private decimal? CalculateRealizedPnL(Order order)
        {
            // Simplified calculation - in reality this would need cost basis tracking
            // This is just a placeholder
            return null;
        }
    }
}
