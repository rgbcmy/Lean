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

            // This is a simplified implementation
            // In a real implementation, this would query historical equity snapshots from the database
            var account = await _unitOfWork.BrokerAccounts.GetByIdAsync(brokerAccountId, cancellationToken);

            if (account == null)
            {
                throw new InvalidOperationException($"Broker account {brokerAccountId} not found");
            }

            var positions = await GetPositionsAsync(brokerAccountId, cancellationToken);
            
            // For now, return a simple equity curve with current values
            // TODO: Implement historical equity tracking
            var response = new EquityCurveResponse
            {
                InitialEquity = 100000m, // TODO: Get from account settings
                CurrentEquity = positions.TotalPortfolioValue,
                TotalReturn = 0, // TODO: Calculate from historical data
                MaxDrawdown = 0, // TODO: Calculate from historical data
                DataPoints = new List<EquityCurvePoint>
                {
                    new EquityCurvePoint
                    {
                        Date = DateTime.UtcNow,
                        Equity = positions.TotalPortfolioValue,
                        Cash = positions.CashBalance,
                        PositionsValue = positions.TotalMarketValue,
                        CumulativeReturn = 0 // TODO: Calculate
                    }
                }
            };

            return response;
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
